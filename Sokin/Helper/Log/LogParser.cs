using AventStack.ExtentReports;
using Sokin.Helper.Networks;
using Sokin.Helper.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Log
{
    class LogParser
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public string Ip;
        public string Port;
        public string UserName;
        public string Password;
        public string Path;
        public string Filename;
        public ReportMethod ReportObj;
        public ExtentTest test;


        public LogParser(string Ip, string Port, string UserName, string Password, string Path, string Filename)
        {

            this.Ip = Ip;
            this.Port = Port;
            this.UserName = UserName;
            this.Password = Password;
            this.Path = Path;
            this.Filename = Filename;
        }
        public List<Message_XlatorIn_0X10> ListXlatorIn_0X10 = new List<Message_XlatorIn_0X10>();
        public List<Message_XlatorIn_0X00> ListXlatorIn_0X00 = new List<Message_XlatorIn_0X00>();
        public List<Message_ThreadTXN_0X10> ListMessageThreadTXN_0X10 = new List<Message_ThreadTXN_0X10>();
        public List<Message_XlatorOut_0X00> ListMessageXlatorOut_0X00 = new List<Message_XlatorOut_0X00>();

        public void LogParsingFunction(string RRN)
        {
            string ScriptContent = File.ReadAllText(Filename);
            ScriptContent = ScriptContent.Replace("$RRN", RRN);
            ScriptContent = ScriptContent.Replace("$PATH", Path);
            File.WriteAllText(Filename, ScriptContent);




            //  Operations on Core

            SSH sh = new SSH();

            //Copy File to Core
            sh.CopyFileToRemoteMachine(this.Ip, this.Port, this.UserName, this.Password, this.Filename, this.Path);

            //  Execute File on Core
            sh.ExecuteFile(this.Ip, this.UserName, this.Password, this.Path, "", this.Filename);

            // Copy Logs From Core
            sh.CopyFolderFromRemoteMachine(this.Ip, this.Port, this.UserName, this.Password, this.Path + "/TRAN_LOG", "CoreLogs");






            // Read all  Log files from Folder find Log Id from File Names
            string[] logfiles = Directory.GetFiles("CoreLogs");






            // Unique Log Files Name
            string[] UniqueLogFileNames = logfiles.Select(v => v.Split('_'))
                .SelectMany(v => v)
                .Where(v => v.Contains("CoreLogs"))
                .Distinct()
                .OrderBy(v => v.Length)
                .ToArray();

            // Unique Log Ids
            string[] UniqueLogIds = logfiles.Select(v => v.Split('_'))
                .SelectMany(v => v)
                .Where(v => !v.Contains("CoreLogs"))
                .Distinct()
                .OrderBy(v => v.Length)
                .ToArray();


            string[] Threadlogs = logfiles.Select(v => v.Split('_'))
              .SelectMany(v => v)
              .Where(v => v.Contains("CoreLogs\\Thread-TXN"))
              .OrderBy(v => v.Length)
              .ToArray();




            Dictionary<string, string> message_type_tlogid = new Dictionary<string, string>();

            for (int j = 0; j < Threadlogs.Count(); j++)
            {


                string[] file = File.ReadAllLines(Threadlogs[j] + "_" + UniqueLogIds[j]);
                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[j] + " Process message called for PE")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[j] + " THREADTIME"));
                foreach (var line in desiredLines)
                {
                    if (line.Contains("Message type received"))
                    {
                        string[] temp = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        string[] temp1 = temp[11].Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                        message_type_tlogid.Add(temp1[0], UniqueLogIds[j]); //getting third bit e.g 0210 key=1 and 0200 key=0
                        break;
                    }
                }
            }

            try
            {
                for (int i = 0; i < UniqueLogIds.Count(); i++)
                {
                    for (int j = 0; j < UniqueLogFileNames.Count(); j++)
                    {


                        //Reading Translator In 
                        if (UniqueLogFileNames[j].Contains("Xlator-IN"))
                        {

                            // reading In of message 0210
                            if (UniqueLogIds[i] == message_type_tlogid["0210"])
                            {
                                logger.Info("Reading Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X10");
                                ReportObj.testInfo(test, "LOGPARSER", "Reading Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X10");
                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);
                                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Parsing Header")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " message sent to socket"));


                                foreach (var v in desiredLines)
                                {

                                    if (!string.IsNullOrEmpty(v))
                                    {
                                        try
                                        {
                                            string[][] parsedHeaderandField = v.Split(new string[] { "Position = " }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => s.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)
                                                 ).ToArray();


                                            if (parsedHeaderandField.Length > 1)
                                            {
                                                Message_XlatorIn_0X10 Message_XlatorIn_0X10 = new Message_XlatorIn_0X10();
                                                Message_XlatorIn_0X10.Position = parsedHeaderandField[1].Length > 0 ? parsedHeaderandField[1][0] : string.Empty;
                                                if (parsedHeaderandField[1].Length > 1)
                                                {
                                                    string[] DEandName = parsedHeaderandField[1][1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                                    Message_XlatorIn_0X10.DataElement = DEandName.Length > 0 ? DEandName[0] : string.Empty;
                                                    Message_XlatorIn_0X10.Name = DEandName.Length > 1 ? DEandName[1] : string.Empty;
                                                }
                                                Message_XlatorIn_0X10.Size = parsedHeaderandField[1].Length > 2 ? parsedHeaderandField[1][2] : string.Empty;
                                                Message_XlatorIn_0X10.Value = parsedHeaderandField[1].Length > 3 ? parsedHeaderandField[1][3] : string.Empty;

                                                ListXlatorIn_0X10.Add(Message_XlatorIn_0X10);

                                                //Console.WriteLine("IncomingMessage.Position : [" + IncomingMessage.Position
                                                //    + "] IncomingMessage.DataElement : [" + IncomingMessage.DataElement
                                                //    + "] IncomingMessage.Name : [" + IncomingMessage.Name
                                                //    + "] IncomingMessage.Size : [" + IncomingMessage.Size
                                                //    + "] IncomingMessage.Value : [" + IncomingMessage.Value + "]");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Error(ex.ToString() + Environment.NewLine);
                                            logger.Error("Some Error occured in parsing header field in Xlator-IN message 0X10");
                                        }
                                    }
                                }

                                string[] subfield = desiredLines.Where(stringToCheck => stringToCheck.Contains("Subfields parsed successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (subfield.Length > 0)
                                {
                                    logger.Info("Subfields parsed successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Subfields parsed successfully");
                                    foreach (var line in subfield)
                                    {
                                        logger.Info(line);
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Subfields parsed successfully'");
                                    logger.Info("Either Subfields Parsing Failed or we are Unable to find string 'Subfields parsed successfully' in logs ");
                                }

                                string[] mandatoryfield = desiredLines.Where(stringToCheck => stringToCheck.Contains("Mandatory fields validated successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (mandatoryfield.Length > 0)
                                {

                                    logger.Info("Mandatory fields validated successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Mandatory fields validated successfully");
                                    foreach (var line in mandatoryfield)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Mandatory fields validated successfully'");
                                    logger.Info("Either Mandatory fields validation Failed or we are unable to find string 'Mandatory fields validated successfully'");
                                }

                                string[] transactionfieldparsing = desiredLines.Where(stringToCheck => stringToCheck.Contains("Transaction specific fields parsed successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (transactionfieldparsing.Length > 0)
                                {
                                    logger.Info("Transaction specific fields parsed successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Transaction specific fields parsed successfully");
                                    foreach (var line in transactionfieldparsing)
                                    {
                                        logger.Info(line);
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Either Transaction specific fields parsing Failed or we are Unable to find string " + "Failed Parsing Transaction specific fields");
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string " + "Transaction specific fields parsed successfully");
                                }


                                string[] transactionfieldvalidation = desiredLines.Where(stringToCheck => stringToCheck.Contains("Transaction specific mandatory fields validated successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (transactionfieldvalidation.Length > 0)
                                {
                                    logger.Info("Transaction specific mandatory fields validated successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Transaction specific mandatory fields validated successfully");
                                    foreach (var line in transactionfieldvalidation)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Either Failed Transaction specific mandatory fields validation or we are unable to find 'Transaction specific mandatory fields validated successfully'");
                                    logger.Info("Unable to find string 'Transaction specific mandatory fields validated successfully'");
                                }


                                string[] messagefieldparsing = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message fields parsed successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagefieldparsing.Length > 0)
                                {
                                    logger.Info("Message fields parsed successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Message fields parsed successfully");
                                    foreach (var line in messagefieldparsing)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Message fields parsed successfully' ");
                                    logger.Info("Unable to find string 'Message fields parsed successfully' ");

                                }

                                string[] messageprocessing = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message processed successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messageprocessing.Length > 0)
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Message fields processed successfully");
                                    foreach (var line in messageprocessing)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Failed Processing Message");
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Message fields processed successfully'");
                                }


                                string[] messagesenttoqueue = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message sent successfully on queue to PE")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagesenttoqueue.Length > 0)
                                {

                                    logger.Info("Message sent successfully on queue to PE");
                                    ReportObj.testInfo(test, "LOGPARSER", "Message sent successfully on queue to PE");
                                    foreach (var line in messagesenttoqueue)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(("Message sent successfully on queue to PE"));

                                    }
                                }
                                else
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Message sent successfully on queue to PE' ");
                                    logger.Info("Unable to find string 'Message sent successfully on queue to PE' ");
                                }


                            }

                            // reading -IN of message 0X00
                            else if (UniqueLogIds[i] == message_type_tlogid["0200"])
                            {
                                Message_XlatorIn_0X00 Message_XlatorIn_0X00 = new Message_XlatorIn_0X00();
                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);

                                logger.Info("Reading Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                ReportObj.testInfo(test, "LOGPARSER", ("Reading Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00"));


                                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Setting message format")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " message sent to socket"));

                                #region setting values from logs

                                string[][] DbIds = desiredLines.Where(stringToCheck => stringToCheck.Contains("ATM device found with ATM Id")).Select(s => s.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

                                if (DbIds.Length > 0)
                                {
                                    Message_XlatorIn_0X00.ChannelId = DbIds[0][3];
                                    Message_XlatorIn_0X00.NetworkId = DbIds[1][3];
                                    Message_XlatorIn_0X00.ProcessGroupId = DbIds[2][3];
                                    Message_XlatorIn_0X00.KeyProfileId = DbIds[3][3];
                                }
                                else
                                {
                                    logger.Info("Unable to get Channel,Network,ProcessGroup,KeyProfileId from" + "Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                }
                                string[][] EmvTags = desiredLines.Where(stringToCheck => stringToCheck.Contains("Tag =")).Select(s => s.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

                                if (EmvTags.Length > 0)
                                {
                                    for (int k = 0; k < EmvTags.Length; k++)
                                    {
                                        Message_XlatorIn_0X00.Tag = EmvTags[k][1];
                                        Message_XlatorIn_0X00.Value = EmvTags[k][3];
                                        ListXlatorIn_0X00.Add(Message_XlatorIn_0X00);
                                    }
                                }
                                else
                                {
                                    logger.Info("I don't know transaction is EMV or not but EMV tags not found in " + "Xlator-IN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                }

                                #endregion

                                string[] finishparsingmessage = desiredLines.Where(stringToCheck => stringToCheck.Contains("Finished parsing message")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (finishparsingmessage.Length > 0)
                                {
                                    logger.Info("Finished parsing message");
                                    ReportObj.testInfo(test, "LOGPARSER", "Finished parsing message");
                                    foreach (var line in finishparsingmessage)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Finished parsing message'");
                                    logger.Info("Unable to find string 'Finished parsing message'");
                                }


                                string[] messagesentsuccessfully = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message sent successfully on queue to PE")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagesentsuccessfully.Length > 0)
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Message sent successfully on queue to PE");
                                    foreach (var line in messagesentsuccessfully)
                                    {

                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Message sent successfully on queue to PE' ");
                                    logger.Info("Unable to send message on queue to pe ");
                                }


                            }

                        }
                        else if (UniqueLogFileNames[j].Contains("Thread-TXN")) // reading thread
                        {

                            // reading Thread of message 0X10
                            if (UniqueLogIds[i] == message_type_tlogid["0210"])
                            {


                                logger.Info("Reading Thread-TXN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X10");
                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);
                                file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);
                                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Process message called for PE")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " THREADTIME"));



                                Message_ThreadTXN_0X10 Message_ThreadTXN_0X10 = new Message_ThreadTXN_0X10();
                                string[] responsecode = desiredLines.Where(stringToCheck => stringToCheck.Contains("ResponseCode")).ToArray();
                                if (responsecode.Length > 0)
                                {

                                    string[] responsecodeline = responsecode.Where(stringToCheck => stringToCheck.Contains("sendToAcquirer")).ToArray();


                                    if (responsecodeline.Length > 0)
                                    {

                                        string[] temp = responsecodeline[0].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                        string[] temp1 = temp[9].Length > 1 ? temp[9].Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { };
                                        if (temp1.Length > 0)
                                        {
                                            Message_ThreadTXN_0X10.ResponseCode = temp1[0];
                                            if (Message_ThreadTXN_0X10.ResponseCode.Equals("000"))
                                            {
                                                logger.Info("Response Code 000 found in  in Thread-TXN log with log id " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X10"); ;
                                                ReportObj.testInfo(test, "LOGPARSER", responsecode[0]);
                                            }
                                            else
                                            {
                                                Message_ThreadTXN_0X10.ResponseCode = temp1[0];
                                                logger.Info(("Response Code " + Message_ThreadTXN_0X10.ResponseCode + " in Thread-TXN log with log id " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X10"));
                                                ReportObj.testInfo(test, "LOGPARSER", responsecode[0]);
                                            }

                                        }
                                    }
                                }



                                string[] messagesentxlo = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message sent to Queue = [XLO]")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagesentxlo.Length > 0)
                                {
                                    logger.Info("Message has been sent to XLO ");
                                    ReportObj.testInfo(test, "LOGPARSER", "Message has been sent to XLO ");
                                    foreach (var line in messagesentxlo)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Unable to find string 'Message sent to Queue = [XLO]'");
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Message sent to Queue = [XLO]");
                                }



                            }
                            //  reading -Thread of message 0X00
                            else if (UniqueLogIds[i] == message_type_tlogid["0200"])
                            {
                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);
                                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Process message called for PE")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " THREADTIME"));

                                string[] accountinfo = desiredLines.Where(stringToCheck => stringToCheck.Contains("Account Successfully Validated")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (accountinfo.Length > 0)
                                {
                                    logger.Info("Account Information Sccessfully validated");
                                    ReportObj.testInfo(test, "LOGPARSER", "Account Information Sccessfully validated");

                                    foreach (var line in accountinfo)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {

                                    logger.Info("Unable to find string 'Account Information Sccessfully validated'");
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to find string 'Account Information Sccessfully validated'");
                                }

                                logger.Info("Going to Validate HSM commands and response from " + "Thread with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                ReportObj.testInfo(test, "LOGPARSER", "Going to Validate HSM commands and response from " + "Thread with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                var hsmvalidationlines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " isCVVRequired_")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " THREADTIME"));
                                //   var hsmvalidationlines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " isCVVRequired_ = [1]")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " THREADTIME"));
                                string[] hsm = hsmvalidationlines.ToArray();


                                int hsmlinesindx = 0;
                                string send_request_code = null;
                                // string response_code;
                                foreach (var hsmlines in hsm)
                                {
                                    if (hsmlines.Contains("isCVVRequired_ = [1]"))
                                    {
                                        //conditions and all check for cvv validation comes here
                                        // Console.WriteLine(hsmlines[hsmlinesindx]);
                                    }
                                    //else if (hsmlines.Contains("isCVVRequired_ = [0]"))
                                    //{ }


                                    if (hsmlines.Contains(" Processing validation request code "))
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", hsmlines);
                                        logger.Info(hsmlines);
                                        string[] temp = hsmlines.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                                        send_request_code = temp[1];

                                    }
                                    //else
                                    //{
                                    //    Console.WriteLine("There is some error before sending  command and in processing validation request code");
                                    //}



                                    if (hsmlines.Contains(" Sending validation command = "))
                                    {
                                        string[] temp = hsmlines.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                                        string hsmcommand = temp[1];
                                        ReportObj.testInfo(test, "LOGPARSER", hsmlines);
                                        logger.Info(hsmlines);
                                        //  Console.WriteLine("HSM Command is" + hsmcommand);
                                    }



                                    if (hsmlines.Contains(" Validation request successful for ="))
                                    {

                                        string[] temp = hsmlines.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (send_request_code == temp[1])
                                        {
                                            logger.Info("Response from the HSM found for Request Code " + send_request_code);
                                            string response = temp[3];
                                            ReportObj.testInfo(test, "LOGPARSER", hsmlines);
                                            logger.Info("Response is : " + response);
                                        }
                                        else
                                        {

                                            ReportObj.testInfo(test, "LOGPARSER", "Response from HSM for Request Code " + send_request_code + "Not found , Hsm Did not respond");
                                            logger.Info("Response from HSM for Request Code " + send_request_code + "Not found , Hsm Did not respond");


                                        }

                                    }
                                    // if validation 
                                    //else
                                    //{
                                    //    Console.WriteLine("Check Core Logs ! Some error occuring in validating hsm command from hsm");
                                    //}

                                    hsmlinesindx++;
                                }


                                ReportObj.testInfo(test, "LOGPARSER", "HSM Level Validation complete from ThreadTXN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                logger.Info("HSM Level Validation complete from ThreadTXN with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");

                            }

                        }

                        //code for reading translator out
                        else if (UniqueLogFileNames[j].Contains("Xlator-OUT")) //reading out
                        {

                            if (UniqueLogIds[i] == message_type_tlogid["0210"])
                            {
                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);

                            }
                            else if (UniqueLogIds[i] == message_type_tlogid["0200"])
                            {
                                Message_XlatorOut_0X00 Message_XlatorOut_0X00 = new Message_XlatorOut_0X00();
                                logger.Info("Reading Xlator-OUT with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                ReportObj.testInfo(test, "LOGPARSER", "Reading Xlator-OUT with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");

                                string[] file = File.ReadAllLines(UniqueLogFileNames[j] + "_" + UniqueLogIds[i]);
                                var desiredLines = file.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Setting message format")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " message sent to socket"));

                                string[] institutioncustomization = desiredLines.Where(stringToCheck => stringToCheck.Contains("Institution based customization done successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (institutioncustomization.Length > 0)
                                {

                                    logger.Info("STEP 1: Institution based customization done successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "STEP 1: Institution based customization done successfully");
                                    foreach (var line in institutioncustomization)
                                    {
                                        logger.Info(line);
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        Console.WriteLine(line);
                                    }
                                }
                                else
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Institution based customization failed");
                                }
                                string[] outgoingfields = desiredLines.Where(stringToCheck => stringToCheck.Contains("Outgoing fields mapped successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (outgoingfields.Length > 0)
                                {
                                    logger.Info("STEP 3: Outgoing fields mapped successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "STEP 3: Outgoing fields mapped successfully");
                                    foreach (var line in outgoingfields)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Error in mapping outgoing fields");
                                    ReportObj.testInfo(test, "LOGPARSER", "Error in mapping outgoing fields");
                                }
                                string[] subfields = desiredLines.Where(stringToCheck => stringToCheck.Contains("Subfields built successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (subfields.Length > 0)
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "STEP 4: Subfields built successfully");
                                    logger.Info("STEP 4: Subfields built successfully");
                                    foreach (var line in subfields)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {
                                    ReportObj.testInfo(test, "LOGPARSER", "Error in building  subfields");
                                    logger.Info("Error in building  subfields");
                                }
                                var messagefieldslines = desiredLines.SkipWhile(s => !s.Contains(UniqueLogIds[i] + " Start building message")).Skip(0).TakeWhile(s => !s.Contains(UniqueLogIds[i] + " Message built successfully"));
                                string[][] MessageFields = messagefieldslines.Where(stringToCheck => stringToCheck.Contains("Position =")).Select(s => s.Split(new string[] { "[", "]", "Position =" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
                                logger.Info("Parsing Message Fields of Xlator_OUT and message 0X00");
                                try
                                {
                                    if (MessageFields.Length > 0)
                                    {
                                        for (int k = 0; k < MessageFields.Length; k++)
                                        {
                                            Message_XlatorOut_0X00.Position = MessageFields[k][4];
                                            Message_XlatorOut_0X00.Length = MessageFields[k][1];
                                            Message_XlatorOut_0X00.Value = MessageFields[k][7];
                                            string[] DEandName = MessageFields[k][5].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                            Message_XlatorOut_0X00.DataElement = DEandName[0];
                                            Message_XlatorOut_0X00.Name = DEandName[1];
                                            ListMessageXlatorOut_0X00.Add(Message_XlatorOut_0X00);
                                        }
                                    }
                                    else
                                    {
                                        logger.Info("Some Error Occur in Parsing Message Fields from Logs from" + "Xlator-Out with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("Some Error Occur in Parsing Message Fields from Logs from" + "Xlator-Out with Tlog-ID " + UniqueLogIds[i] + " and logFilename " + UniqueLogFileNames[j] + " and Message type 0X00" + Environment.NewLine);
                                    logger.Info((ex.ToString()));
                                }
                                string[] messagebuild = desiredLines.Where(stringToCheck => stringToCheck.Contains("Raw message built successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagebuild.Length > 0)
                                {
                                    logger.Info("STEP 5: Message built successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "STEP 5: Message built successfully");
                                    foreach (var line in messagebuild)
                                    {
                                        logger.Info(line);
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Error inh builiding messages");
                                    ReportObj.testInfo(test, "LOGPARSER", "Error inh builiding messages");
                                }
                                string[] outgoingmessageprocessed = desiredLines.Where(stringToCheck => stringToCheck.Contains("Outgoing message processed successfully")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (outgoingmessageprocessed.Length > 0)
                                {
                                    logger.Info(" Outgoing message processed successfully");
                                    ReportObj.testInfo(test, "LOGPARSER", "Outgoing message processed successfully");
                                    foreach (var line in outgoingmessageprocessed)
                                    {
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                        logger.Info(line);
                                    }
                                }
                                else
                                {

                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to Process Outgoing Message");
                                    logger.Info("Unable to Process Outgoing Message");
                                }
                                string[] messagesentqueue = desiredLines.Where(stringToCheck => stringToCheck.Contains("Message sent to Queue ")).Select(stringtoCheck => stringtoCheck).ToArray();
                                if (messagesentqueue.Length > 0)
                                {
                                    logger.Info("Message sent to Queue");
                                    ReportObj.testInfo(test, "LOGPARSER", "Message sent to Queue");
                                    foreach (var line in messagesentqueue)
                                    {
                                        logger.Info(line);
                                        ReportObj.testInfo(test, "LOGPARSER", line);
                                    }
                                }
                                else
                                {
                                    logger.Info("Unable to send message to queue");
                                    ReportObj.testInfo(test, "LOGPARSER", "Unable to send message to queue");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info("Error in Reading Log" + Environment.NewLine);
            }
        }
    }
    public class Message_XlatorIn_0X10
    {
        public string Position { get; set; }
        public string DataElement { get; set; }

        public string Name { get; set; }
        public string Size { get; set; }
        public string Value { get; set; }

    }
    public class Message_XlatorIn_0X00
    {
        public string ChannelId { get; set; }
        public string NetworkId { get; set; }
        public string KeyProfileId { get; set; }
        public string ProcessGroupId { get; set; }

        public string Tag { get; set; }
        public string Value { get; set; }


    }
    public class Message_ThreadTXN_0X10
    {
        public string ResponseCode;
        public string stan;
        public string RRN;
    }
    public class Message_ThreadTXN_0X00
    { }
    public class Message_XlatorOut_0X10
    {

    }
    public class Message_XlatorOut_0X00
    {
        public string Position { get; set; }
        public string DataElement { get; set; }

        public string Name { get; set; }
        public string Length { get; set; }
        public string Value { get; set; }
    }
}
