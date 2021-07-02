using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LotAutorunCheck
{
    class Program
    {
        const string BASE_PATH = @"D:\";

        const string RESULT_PATH = @"result";


        static void Main(string[] args)
        {
            Console.WriteLine("Loading lotlist.txt..");
            try
            {
                Dictionary<String, List<OTPCheck>> motherLots = LoadMotherLots();
                motherLots = LoadOTP(motherLots);

                foreach (var key in motherLots.Keys)
                {
                    CheckLot(motherLots[key]);
                }

                WriteLotToCSV(motherLots);

                Console.WriteLine("All complete, please check result folder");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            Console.ReadLine();
        }

        private static void WriteLotToCSV(Dictionary<string, List<OTPCheck>> motherLots)
        {
            String path = Path.Combine(Environment.CurrentDirectory, RESULT_PATH);
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            foreach (var motherLot in motherLots)
            {
                WriteOutputFile(motherLot, path);
            }
        }

        static void WriteOutputFile(KeyValuePair<string, List<OTPCheck>> pair, String path)
        {
            String fileName = pair.Key + ".csv";
            path = Path.Combine(path, fileName);

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(String.Format("lotID,OTP,Autorun1,Autorun2,Autorun3,Autorun4,Pass/Fail"));
                foreach (var check in pair.Value)
                {
                    sw.WriteLine(String.Format("{0},{1},{2},{3},{4},{5},{6}",check.LotID, check.OTP, check.Autorun[0], check.Autorun[1], check.Autorun[2], check.Autorun[3], check.AutorunResult == true? "PASS" : "FAIL"));
                }
            }

            Console.WriteLine("Result output to " + path);
        }

        static void CheckLot(List<OTPCheck> otpChecks)
        {
            String currentLotID = null;
            List<String> fileNames = null;
            foreach (var check in otpChecks)
            {
                if(currentLotID == null || check.LotID != currentLotID )
                {
                    currentLotID = check.LotID;
                    fileNames = getFileNames(check.LotID);
                }

                if (fileNames.Count == 0) continue;

                var matchedFiles = fileNames.Where(f => f.Contains(check.OTP));
                foreach (var file in matchedFiles)
                {
                    int autorun = getAutorunInfo(file);
                    check.Autorun[autorun-1] = true;
                }

                check.AutorunResult = true;
                for (int i = 0; i < check.Autorun.Length; i++)
                {
                    check.AutorunResult = check.AutorunResult & check.Autorun[i];
                }
            }
        }

        static int getAutorunInfo(String fileName)
        {
            int index = fileName.IndexOf(".zip");  //Q1SV85.1-W3RT3_[OV10642DU_FT3_60C_aCSP_RF0101]_40b480fb90a20a380043f800000017ad_Image_12_Light_L_maxExpo_21xHighGain_RED_1.zip_20210628155834.zip
            String name = fileName.Substring(0, index);   //Q1SV85.1-W3RT3_[OV10642DU_FT3_60C_aCSP_RF0101]_40b480fb90a20a380043f800000017ad_Image_12_Light_L_maxExpo_21xHighGain_RED_1
            String[] splitor = name.Split('_');
            return Convert.ToInt32(splitor[splitor.Length - 1]);
        }
        static List<String> getFileNames(String lotID)
        {
            String path;
            path = Path.Combine(BASE_PATH, lotID);
            DirectoryInfo directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                return new List<string>();
            }

            DirectoryInfo tpFolders = directory.GetDirectories().ToList().Where(d => d.Name.EndsWith("RF0101")).FirstOrDefault();
            path = tpFolders.ToString();

            DirectoryInfo toFolder = new DirectoryInfo(path);
            FileInfo[] fileInfos = tpFolders.GetFiles();

            List<String> fileNames = new List<string>();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                fileNames.Add(fileInfos[i].Name);
            }
            return fileNames;
        }

        static Dictionary<String, List<OTPCheck>> LoadMotherLots()
        {
            Dictionary<String, List<OTPCheck>> motherLotCheck = new Dictionary<String, List<OTPCheck>>();

            String line = null;
            using (StreamReader sr = new StreamReader("lotlist.txt"))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    motherLotCheck.Add(line.Trim(), null);
                }
            }
            return motherLotCheck;
        }

        static Dictionary<String, List<OTPCheck>> LoadOTP(Dictionary<String, List<OTPCheck>> motherLots)
        {
            Dictionary<String, List<OTPCheck>> motherLotsCopyed = new Dictionary<string, List<OTPCheck>>();

            foreach (var motherLot in motherLots)
            {
                motherLotsCopyed.Add(motherLot.Key, null);
            }

            foreach (var motherLot in motherLots)
            {
                motherLotsCopyed[motherLot.Key] = FillOTPInfo(motherLot.Key);
            }

            return motherLotsCopyed;
        }

        static List<OTPCheck> FillOTPInfo(String motherLot)
        {
            List<OTPCheck> otpChecks = new List<OTPCheck>();
            String path = Path.Combine(Environment.CurrentDirectory, "csv", motherLot + ".csv");

            using (StreamReader sr = new StreamReader(path))
            {
                String line;
                bool isReachStartLine = false;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("OTP check"))
                    {
                        isReachStartLine = true;
                        continue;
                    }

                    if (!isReachStartLine) continue;

                    otpChecks.Add(GetOTPInfoFromLine(line));
                }
            }
            return otpChecks;
        }

        static OTPCheck GetOTPInfoFromLine(String line)
        {
            String[] splits = line.Split(',');
            String temp = splits[0]; //3:8:Q1UB79.1-W3RT2:21
            String lotID = temp.Split(':')[2];
            String otp = splits[1];

            OTPCheck otpCheck = new OTPCheck()
            {
                LotID = lotID,
                OTP = otp
            };
            return otpCheck;
        }
    }
}
