self.Module = {};
Module.print = msg => console.log(`WASM-WORKER: ${msg}`);
Module.printErr = msg => {
    console.error(`WASM-WORKER: ${msg}`);
};
Module.preRun = [];
Module.postRun = [];
Module.preloadPlugins = [];

Module.locateFile = fileName => {
    switch (fileName) {
        case 'dotnet.wasm': return self.MN.baseUrl+'dotnet.wasm';
        default: return fileName;
    }
};

Module.preRun.push(() => {
    const mono_wasm_add_assembly = Module.cwrap('mono_wasm_add_assembly', null, [
        'string',
        'number',
        'number',
    ]);

    mono_string_get_utf8 = Module.cwrap('mono_wasm_string_get_utf8', 'number', ['number']);

    MONO.loaded_files = [];

     self.MN.AssemblyFilenames.forEach(url => {

        const runDependencyId = `blazor:${url}`;
        addRunDependency(runDependencyId);

         asyncLoad(self.MN.baseUrl + url).then(
            data => {
                const heapAddress = Module._malloc(data.length);
                const heapMemory = new Uint8Array(Module.HEAPU8.buffer, heapAddress, data.length);
                heapMemory.set(data);
                mono_wasm_add_assembly(url, heapAddress, data.length);
                MONO.loaded_files.push(url);
                removeRunDependency(runDependencyId);
            },
            errorInfo => {
                const isPdb404 = errorInfo instanceof XMLHttpRequest
                    && errorInfo.status === 404
                    && url.match(/\.pdb$/);
                if (!isPdb404) {
                    onError(errorInfo);
                }
                removeRunDependency(runDependencyId);
            }
        );
    });
});

Module.postRun.push(() => {
    MONO.mono_wasm_setenv('MONO_URI_DOTNETRELATIVEORABSOLUTE', 'true');
    const load_runtime = Module.cwrap('mono_wasm_load_runtime', null, ['string', 'number']);
    load_runtime('appBinDir', 0);
    MONO.mono_wasm_runtime_is_ready = true;
    self.MN.RunAction = Module.mono_bind_static_method('[Monsajem_incs_WASM]WebAssembly.Browser.MonsajemDomHelpers.WebProcess:RunAction');
    self.MN.RunFunction = Module.mono_bind_static_method('[Monsajem_incs_WASM]WebAssembly.Browser.MonsajemDomHelpers.WebProcess:RunFunction');
    self.MN.RunFunctionTask = Module.mono_bind_static_method('[Monsajem_incs_WASM]WebAssembly.Browser.MonsajemDomHelpers.WebProcess:RunFunctionTask');
    self.MN.RunFunctionTaskResult = Module.mono_bind_static_method('[Monsajem_incs_WASM]WebAssembly.Browser.MonsajemDomHelpers.WebProcess:RunFunctionTaskResult');
    Module.mono_bind_static_method('[Monsajem_incs_WASM]WebAssembly.Browser.MonsajemDomHelpers.WebProcess:ThisIsInWorker')();
});

function asyncLoad(url, reponseType) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        const arrayBufferType = 'arraybuffer';
        xhr.open('GET', url, /* async: */ true);
        xhr.responseType = reponseType || arrayBufferType;
        xhr.onload = function xhr_onload() {
            if (xhr.status == 200 || xhr.status == 0 && xhr.response) {
                if (this.responseType === arrayBufferType) {
                    const asm = new Uint8Array(xhr.response);
                    resolve(asm);
                } else {
                    resolve(xhr.response);
                }
            } else {
                reject(xhr);
            }
        };
        xhr.onerror = reject;
        xhr.send(undefined);
    });
}

self.importScripts(self.MN.baseUrl+`dotnet.5.0.0.js`);