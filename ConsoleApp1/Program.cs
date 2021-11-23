using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using NetTopologySuite.Operation.OverlayNG;

public sealed class MyObject : MarshalByRefObject
{
    private static readonly Assembly MyAssembly = typeof(MyObject).Assembly;

    public double GetValue()
    {
        return PrecisionUtility.InherentScale(100);
    }

    private static void Main()
    {
        var appDomainSetup = new AppDomainSetup { ApplicationBase = "." };

        var permissionSet = new PermissionSet(PermissionState.None);
        permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution | SecurityPermissionFlag.SkipVerification));

        var appDomain = AppDomain.CreateDomain("Sandbox", null, appDomainSetup, permissionSet);
        try
        {
            var myObject = (MyObject)appDomain.CreateInstanceAndUnwrap(MyAssembly.FullName, typeof(MyObject).FullName);
            Console.WriteLine(myObject.GetValue());
        }
        finally
        {
            AppDomain.Unload(appDomain);
        }
    }
}
