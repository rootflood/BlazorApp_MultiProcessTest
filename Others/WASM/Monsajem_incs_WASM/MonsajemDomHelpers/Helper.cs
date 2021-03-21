using System;
using System.Collections.Generic;
using System.Text;
using static Monsajem_Incs.ArrayExtentions.ArrayExtentions;
using WebAssembly.Browser.DOM;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.JSInterop;
using System.IO;
using Monsajem_Incs.Serialization;

namespace WebAssembly.Browser.MonsajemDomHelpers
{
    public static class js
    {
        public static Document Document = Document.document;
        public static IJSUnmarshalledRuntime IJSUnmarshalledRuntime;
        private static Action OnPopState;
        private static Action[] MyHistorySates;
        private static void MakeState()
        {
            if(MyHistorySates==null)
            {
                MyHistorySates=new Action[0];
                Window.window.OnPopState += (c1, c2) => OnPopState();
            }
            OnPopState=()=>Pop(ref MyHistorySates)();
        }
        public static void DropState()
        {
            Pop(ref MyHistorySates)();
        }

        public static void PushState(Action Onpop)
        {
            MakeState();
            Insert(ref MyHistorySates,Onpop);
            Window.window.History.PushState("", "",Window.window.Location.Href);
        }

        public static void LockBack()
        {
            Action Lock =null;
            Lock = () =>
            {
                js.Eval("alert('1');");
                Window.window.History.Go(1);
                PushState(Lock);
                js.Eval("alert('1');");
            };
            PushState(Lock);
        }

        public static void GoBack()
        {
            Window.window.History.Back();
        }

        public static void UnLockBack()
        {
            Pop(ref MyHistorySates);
        }

        public static void Redirect(string Path)
        {
            Window.window.Location.Pathname = Path;
        }


        public static string Js(bool Value)
        {
            if (Value == true)
                return "true";
            return "false";
        }
        public static string Js(int Value)
        {
            return Value.ToString();
        }
        public static string Js(string Value)
        {
            if (Value == null)
                return"''";
            else
                return $"decodeURIComponent(escape(atob('{Convert.ToBase64String(Encoding.UTF8.GetBytes(Value))}')))";
        }

        public static string Eval(string js)
        {
            try
            {
                return Runtime.InvokeJS(js);
            }
            catch(Exception ex)
            {
                throw new Exception("Eval >> " + js, ex);
            }
        }

        public static void EvalGlobal(string js)
        {
            Eval($"var s=document.createElement('script');s.innerHTML={Js(js)};document.body.appendChild(s);");
        }

        public static async Task<byte[]> ReadBytes(this Blob File)
        {
            using (var WebClient = new HttpClient())
                return await WebClient.GetByteArrayAsync(URL.CreateObjectUrl(File));
        }

        public static string ToDataUrl(this byte[] Data)
        {
            return URL.CreateDataUrl(Data);
        }

        public static string ToObjectUrlUnmarshalled(this byte[] Data)
        {
            //return Window.window.Url.CreateObjectUrl(new Blob(Data));
            js.EvalGlobal(
                @"
                var MNObjectUrl='';
                function MNToObjectUrl(ar){
                    ar = Blazor.platform.toUint8Array(ar);
                    MNObjectUrl = URL.createObjectURL(new Blob([ar],{type:'application/octet-stream'}));
                }");
            IJSUnmarshalledRuntime.InvokeUnmarshalled<byte[], object>("MNToObjectUrl", Data);
            return (string)Runtime.GetGlobalObject("MNObjectUrl");
        }

        public static string ToObjectUrl(this byte[] Data)
        {
            return URL.CreateObjectUrl(new Blob(Data));
        }

        public static async Task<string> ToDataUrl(this Blob File)
        {
            return (await File.ReadBytes()).ToDataUrl();
        }

        public static string ToObjectUrl(this Blob File)
        {
            return URL.CreateObjectUrl(File);
        }

        public static byte[] GetImageBytes(this HTMLImageElement img,double quality=1)
        {
            if (quality > 1)
                throw new ArgumentOutOfRangeException("quality should be equal or less than 1.");
            var imgID = img.Id;
            img.Id = "imgMN";
            string URL = Eval(
                @"(function(){
                  var MAX_WIDTH = 500;
                  var width = " + img.NaturalWidth + @";
                  var height = " + img.NaturalHeight + @";
                  height =height*( MAX_WIDTH / width);
                  width = MAX_WIDTH;                            
                  var canvas = document.createElement('canvas');
                  canvas.width = width;
                  canvas.height = height;
                  var ctx = canvas.getContext('2d');
                  ctx.drawImage(document.getElementById('" + img.Id + @"'),0,0, width, height);
                  return canvas.toDataURL('image/jpeg',"+quality.ToString("#.############")+");}).call(null);");
            img.Id = imgID;
            return System.Convert.FromBase64String(URL.Substring(23));
        }

        public static async Task<byte[]> GetImageBytesFast(this HTMLImageElement img, double quality=1)
        {
            if (quality > 1)
                throw new ArgumentOutOfRangeException("quality should be equal or less than 1.");
            var imgID = img.Id;
            img.Id = "imgMN";
            string URL = Eval(
                @"(function(){
                  var MAX_WIDTH = 500;
                  var width = " + img.NaturalWidth + @";
                  var height = " + img.NaturalHeight + @";
                  height =height*( MAX_WIDTH / width);
                  width = MAX_WIDTH;                            
                  var canvas = document.createElement('canvas');
                  canvas.width = width;
                  canvas.height = height;
                  var ctx = canvas.getContext('2d');
                  ctx.drawImage(document.getElementById('" + img.Id + @"'),0,0,width,height);
                  return URL.createObjectURL(canvas.toBlob('image/jpeg'," + quality.ToString("#.############") + "));}).call(null);");
            img.Id = imgID;
            using (var WebClient = new HttpClient())
                return await WebClient.GetByteArrayAsync(URL);
        }
    }

    public class WebProcess
    {
        private readonly static string WebWorkerJs = ((Func<string>)(() => {
            var assembly = typeof(WebWorker).Assembly;
            var resourceName = "Monsajem_incs_WASM.MonsajemDomHelpers.WebWorker.js";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }))();
        private WebWorkerClient Worker;
        public readonly Task IsReady;
        internal static bool IsInWorker;
        private static void ThisIsInWorker()
        {
            IsInWorker = true;
            var AssemblyFilenames =
                (System.Runtime.InteropServices.JavaScript.Array)
                ((JSObject)Runtime.GetGlobalObject("MN")).GetObjectProperty("AssemblyFilenames");
            var AsmLen = AssemblyFilenames.Length;
            for (int i = 0; i < AsmLen; i++)
                Monsajem_Incs.Assembly.Assembly.TryLoadAssembely(((string)AssemblyFilenames[i]));
            js.Eval("postMessage('');");
        }
        
        public WebProcess()
        {
            if (IsInWorker)
                throw new BadImageFormatException("new process can't declare in WebWorker!");
            IsReady = GetReady();
        }
        private async Task GetReady()
        {
            Worker = new WebWorkerClient();
            var AssembellyNames = "[";
            foreach (var Asm in Monsajem_Incs.Assembly.Assembly.AllAppAssemblies)
                AssembellyNames += $"'{Asm.GetName().Name}.dll',";
            if (Monsajem_Incs.Assembly.Assembly.AllAppAssemblies.Length > 0)
                AssembellyNames = AssembellyNames.Substring(0, AssembellyNames.Length - 1);
            AssembellyNames += "]";
            var Location = Window.window.Location;
            await Worker.Run($"self.MN={{}};self.MN.AssemblyFilenames={AssembellyNames};self.MN.baseUrl = '{Location.Protocol}//{Location.Hostname}:{Location.Port}/_framework/';");
            var Ready = Worker.GetMessage();
            await Worker.Run(WebWorkerJs);
            await Ready;
        }

        public async Task Run(Action Action)
        {
            await IsReady;
            Worker.PostMessage("MNData", Uint8Array.From(Action.Serialize()));
            var Message = Worker.GetMessage();
            await Worker.Run($"self.MN.RunAction()");
            await Message;
        }
        public async Task<object> Run(Func<object> Func)
        {
            await IsReady;
            Worker.PostMessage("MNData", Uint8Array.From(Func.Serialize()));
            var Message = Worker.GetMessage();
            await Worker.Run($"self.MN.RunFunction()");
            return (await Message).GetData<Uint8Array>().ToArray().Deserialize<object>();
        }
        public async Task Run(Func<Task> Action)
        {
            await IsReady;
            Worker.PostMessage("MNData", Uint8Array.From(Action.Serialize()));
            var Message = Worker.GetMessage();
            await Worker.Run($"self.MN.RunFunctionTask()");
            await Message;
        }
        public async Task<object> Run(Func<Task<object>> Func)
        {
            await IsReady;
            Worker.PostMessage("MNData", Uint8Array.From(Func.Serialize()));
            var Message = Worker.GetMessage();
            await Worker.Run($"self.MN.RunFunctionTaskResult()");
            return (await Message).GetData<Uint8Array>().ToArray().Deserialize<object>();
        }

        private static void RunAction()
        {
            ((Uint8Array)Runtime.GetGlobalObject("MNData")).ToArray().
                Deserialize<Action>()();
            js.Eval("postMessage('');");
        }
        private static void RunFunction()
        {
            var result = ((Uint8Array)Runtime.GetGlobalObject("MNData")).ToArray().
                            Deserialize<Func<object>>()();
            WebWorker.CurrentWebWorker.PostMessage(Uint8Array.From(result.Serialize()));
        }
        private static void RunFunctionTask()
        {
            ((Uint8Array)Runtime.GetGlobalObject("MNData")).ToArray().
                Deserialize<Func<Task>>()();
            js.Eval($"postMessage('');");
        }
        private static async void RunFunctionTaskResult()
        {
            var result = await ((Uint8Array)Runtime.GetGlobalObject("MNData")).ToArray().
                            Deserialize<Func<Task<object>>>()();
            WebWorker.CurrentWebWorker.PostMessage(Uint8Array.From(result.Serialize()));
        }
    }
}