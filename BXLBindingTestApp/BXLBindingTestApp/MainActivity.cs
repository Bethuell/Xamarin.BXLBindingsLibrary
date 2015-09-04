using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System.IO;
using Jpos.Config;
using Java.Util;

namespace BXLBindingTestApp
{
    [Activity(Label = "BXLBindingTestApp", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            string logicalName = GetNameOfPrinter();

            if (String.IsNullOrEmpty(logicalName))
                Toast.MakeText(this, "Could not find printer. Please make sure your device is paired to the printer via Bluetooth.", ToastLength.Long);

            CreateNewConfigFile();

            var button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += delegate
            {
                Jpos.POSPrinter p = new Jpos.POSPrinter(this);
                p.Open(logicalName); //SPP-R300

                p.Claim(0);
                p.DeviceEnabled = true;
                
                //Print out text. Be sure to have a new line (\n) at the end of the data.
                p.PrintNormal(
                    Jpos.POSPrinterConst.PtrSReceipt,
                    "This is a test.\nLorem Ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum.\nThis is a new line.\n");
                p.Close();
            };
        }

        private String GetNameOfPrinter()
        {
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var bondedDevices = bluetoothAdapter.BondedDevices;

            foreach (var device in bondedDevices)
            {
                if (!String.IsNullOrEmpty(device.Name) && device.Name.Contains("SPP"))  
                    return device.Name;
            }
            return String.Empty;
        }

        private void CreateNewConfigFile()
        {
            var inStream = this.Resources.OpenRawResource(Resource.Raw.jpos);

            try
            {
                var myfolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var mypath = System.IO.Path.Combine(myfolder, "jpos.xml");
                using (var myfile = File.Create(mypath))
                {
                    inStream.CopyTo(myfile);
                }
            }
            finally
            {
                try
                {
                    inStream.Close();
                }
                catch (IOException e)
                {
                }

            }
        }
    }
}

