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
        const int PDF_PAGE_COUNT = 2;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            string logicalName = GetNameOfPrinter();

            if (String.IsNullOrEmpty(logicalName))
                Toast.MakeText(this, "Could not find printer. Please make sure your device is paired to the printer via Bluetooth.", ToastLength.Long).Show();

            CreateNewConfigFile();

            var btnPrintText = FindViewById<Button>(Resource.Id.btnPrintText);
            btnPrintText.Click += delegate
            {
                PrintText(logicalName, btnPrintText);
            };

            var btnPrintPDF = FindViewById<Button>(Resource.Id.btnPrintPDF);
            btnPrintPDF.Click += delegate
            {
                PrintPDF(logicalName, btnPrintPDF);
            };
        }

        /// <summary>
        /// Retrieve the logical name of the printer via Bluetooth. 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Create the jpos.xml config. An entry is required in this file for the printer that is being used.
        /// </summary>
        private void CreateNewConfigFile()
        {
            var inStream = this.Resources.OpenRawResource(Resource.Raw.jpos);

            try
            {
                var personalDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var jposPath = System.IO.Path.Combine(personalDir, "jpos.xml");
                using (var file = File.Create(jposPath))
                {
                    inStream.CopyTo(file);
                }
            }
            finally
            {
                try
                {
                    if (inStream != null)
                        inStream.Close();
                }
                catch (IOException e) { }
            }
        }

        private void PrintText(string printerLogicalName, Button printButton)
        {
            Jpos.POSPrinter p = new Jpos.POSPrinter(this);
            try
            {
                printButton.Enabled = false;

                p.Open(printerLogicalName); //SPP-R300
                p.Claim(0);
                p.DeviceEnabled = true;

                //Print out text. Be sure to have a new line (\n) at the end of the data.
                p.PrintNormal(
                    Jpos.POSPrinterConst.PtrSReceipt,
                    "This is a test.\nLorem Ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum lorum ipsum.\nThis is a new line.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Toast.MakeText(this, "Error:\n" + e.StackTrace, ToastLength.Long).Show();
            }
            finally
            {
                printButton.Enabled = true;

                if (p != null)
                    p.Close();
            }
        }

        private void PrintPDF(string printerLogicalName, Button printButton)
        {
            Jpos.POSPrinter p = new Jpos.POSPrinter(this);
            var inStream = this.Resources.OpenRawResource(Resource.Raw.test_pdf);
            try
            {
                var personalDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var pdfPath = System.IO.Path.Combine(personalDir, "test.pdf");
                using (var file = File.Create(pdfPath))
                {
                    inStream.CopyTo(file);
                }

                printButton.Enabled = false;

                p.Open(printerLogicalName); //SPP-R300
                p.Claim(0);
                p.DeviceEnabled = true;

                for (int pageNumber = 0; pageNumber < PDF_PAGE_COUNT; pageNumber++)
                {
                    //Print PDF
                    p.PrintPDF(
                        Jpos.POSPrinterConst.PtrSReceipt,           //Print Type
                        pdfPath,                                    //Path to PDF file
                        1000,                                       //Width
                        Jpos.POSPrinterConst.PtrPdfLeft,            //Alignment
                        pageNumber,                                 //Page
                        50);                                        //Brightness
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Toast.MakeText(this, "Error:\n" + e.StackTrace, ToastLength.Long).Show();
            }
            finally
            {
                printButton.Enabled = true;
                
                try
                {
                    if (inStream != null)
                        inStream.Close();

                    if (p != null)
                        p.Close();
                }
                catch (IOException e) { }
            }
        }
    }
}

