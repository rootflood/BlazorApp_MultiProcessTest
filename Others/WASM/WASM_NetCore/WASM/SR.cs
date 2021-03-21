// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.Resources;

namespace System
{
    internal partial class SR
    {
#if (!NETSTANDARD1_0 && !NETSTANDARD1_1 && !NET45) // AppContext is not supported on < NetStandard1.3 or < .NET Framework 4.5
        private static readonly bool s_usingResourceKeys = AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out bool usingResourceKeys) ? usingResourceKeys : false;
#endif

        // This method is used to decide if we need to append the exception message parameters to the message when calling SR.Format.
        // by default it returns the value of System.Resources.UseSystemResourceKeys AppContext switch or false if not specified.
        // Native code generators can replace the value this returns based on user input at the time of native code generation.
        // The Linker is also capable of replacing the value of this method when the application is being trimmed.
        private static bool UsingResourceKeys() =>
            s_usingResourceKeys;

        internal static string GetResourceString(string resourceKey, string? defaultString = null)
        {
            if (UsingResourceKeys())
            {
                return defaultString ?? resourceKey;
            }

            string? resourceString = null;
            try
            {
                resourceString =
                    ResourceManager.GetString(resourceKey);
            }
            catch (MissingManifestResourceException) { }

            if (defaultString != null && resourceKey.Equals(resourceString))
            {
                return defaultString;
            }

            return resourceString!; // only null if missing resources
        }

        internal static string Format(string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(resourceFormat, p1);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(resourceFormat, p1, p2);
        }

        internal static string Format(string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(resourceFormat, p1, p2, p3);
        }

        internal static string Format(string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(resourceFormat, args);
            }

            return resourceFormat;
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(provider, resourceFormat, p1);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(provider, resourceFormat, p1, p2);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(provider, resourceFormat, p1, p2, p3);
        }

        internal static string Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(provider, resourceFormat, args);
            }

            return resourceFormat;
        }
    }
}

namespace FxResources.System.Private.Runtime.InteropServices.JavaScript
{
    internal static class SR { }
}
namespace System
{
    internal static partial class SR
    {
        private static global::System.Resources.ResourceManager s_resourceManager;
        internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof(FxResources.System.Private.Runtime.InteropServices.JavaScript.SR)));

        /// <summary>Invalid argument: {0} can not be null.</summary>
        internal static string @ArgumentCannotBeNull => GetResourceString("ArgumentCannotBeNull", @"Invalid argument: {0} can not be null.");
        /// <summary>Invalid argument: {0} can not be null and must have a length</summary>
        internal static string @ArgumentCannotBeNullWithLength => GetResourceString("ArgumentCannotBeNullWithLength", @"Invalid argument: {0} can not be null and must have a length");
        /// <summary>CoreObject Error binding: {0}</summary>
        internal static string @CoreObjectErrorBinding => GetResourceString("CoreObjectErrorBinding", @"CoreObject Error binding: {0}");
        /// <summary>Error releasing object {0}</summary>
        internal static string @ErrorReleasingObject => GetResourceString("ErrorReleasingObject", @"Error releasing object {0}");
        /// <summary>HostObject Error binding: {0}</summary>
        internal static string @HostObjectErrorBinding => GetResourceString("HostObjectErrorBinding", @"HostObject Error binding: {0}");
        /// <summary>JSObject Error binding: {0}</summary>
        internal static string @JSObjectErrorBinding => GetResourceString("JSObjectErrorBinding", @"JSObject Error binding: {0}");
        /// <summary>Multiple handles pointing at jsId: {0}</summary>
        internal static string @MultipleHandlesPointingJsId => GetResourceString("MultipleHandlesPointingJsId", @"Multiple handles pointing at jsId: {0}");
        /// <summary>System.Private.Runtime.InteropServices.JavaScript is not supported on this platform.</summary>
        internal static string @SystemRuntimeInteropServicesJavaScript_PlatformNotSupported => GetResourceString("SystemRuntimeInteropServicesJavaScript_PlatformNotSupported", @"System.Private.Runtime.InteropServices.JavaScript is not supported on this platform.");
        /// <summary>TypedArray is not of correct type.</summary>
        internal static string @TypedArrayNotCorrectType => GetResourceString("TypedArrayNotCorrectType", @"TypedArray is not of correct type.");
        /// <summary>Unable to cast null to type {0}.</summary>
        internal static string @UnableCastNullToType => GetResourceString("UnableCastNullToType", @"Unable to cast null to type {0}.");
        /// <summary>Unable to cast object of type {0} to type {1}.</summary>
        internal static string @UnableCastObjectToType => GetResourceString("UnableCastObjectToType", @"Unable to cast object of type {0} to type {1}.");
        /// <summary>ValueType arguments are not supported.</summary>
        internal static string @ValueTypeNotSupported => GetResourceString("ValueTypeNotSupported", @"ValueType arguments are not supported.");

    }
}

