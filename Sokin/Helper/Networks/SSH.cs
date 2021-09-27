using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Networks
{
    class SSH
    {
        public void CopyFileToRemoteMachine(string ip, string port, string username, string pass, string filename, string uploadPath)
        {
            ConnectionInfo connectionInfo = new PasswordConnectionInfo(ip, int.Parse(port), username, pass);
            try
            {

                //  SftpClient _sftp = new SftpClient("10.30.0.53", 22, "akbl", "tpstps");

                using (ScpClient _scp = new ScpClient(connectionInfo))
                {
                    _scp.Connect();
                    //using ()
                    //{
                    FileInfo FILE = new FileInfo(filename);


                    _scp.Upload(FILE, uploadPath + FILE.Name);
                    //}
                    // UploadFile(_sftp, filename, uploadPath);
                    _scp.Disconnect();
                }
                // 


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
        }
        public void CopyFileFromRemoteMachine(string ip, string port, string username, string pass, string source, string dest)
        {
            try
            {
                //  SftpClient _sftp = new SftpClient("10.30.0.53", 22, "akbl", "tpstps");

                SftpClient _sftp = new SftpClient(ip, int.Parse(port), username, pass);
                _sftp.Connect();
                using (Stream localFile = File.Create(dest))
                {
                    _sftp.DownloadFile(source, localFile);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        public void CopyFolderFromRemoteMachine(string ip, string port, string username, string pass, string source, string dest)
        {
            try
            {
                //  SftpClient _sftp = new SftpClient("10.30.0.53", 22, "akbl", "tpstps");

                SftpClient _sftp = new SftpClient(ip, int.Parse(port), username, pass);
                _sftp.Connect();

                DownloadDirectory(_sftp, source, dest, false);

                _sftp.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }
        }

        public void UploadFile(SftpClient client, string filename, string UploadPath)
        {
            Console.WriteLine("Uploading {0}", filename);

            using (Stream fileStream = File.OpenRead(filename))
            {
                Action<ulong> uploadCallBack = null;
                client.UploadFile(fileStream, UploadPath, true, uploadCallBack);
            }
        }

        public void DownloadDirectory(SftpClient client, string source, string destination, bool recursive = false)
        {

            if (!Directory.Exists(destination))
            {

                Directory.CreateDirectory(destination);
            }
            // List the files and folders of the directory
            var files = client.ListDirectory(source);

            // Iterate over them
            foreach (SftpFile file in files)
            {
                // If is a file, download it
                if (!file.IsDirectory && !file.IsSymbolicLink)
                {
                    DownloadFile(client, file, destination);
                }
                // If it's a symbolic link, ignore it
                else if (file.IsSymbolicLink)
                {
                    Console.WriteLine("Symbolic link ignored: {0}", file.FullName);
                }
                // If its a directory, create it locally (and ignore the .. and .=) 
                //. is the current folder
                //.. is the folder above the current folder -the folder that contains the current folder.
                else if (file.Name != "." && file.Name != "..")
                {
                    var dir = Directory.CreateDirectory(Path.Combine(destination, file.Name));
                    // and start downloading it's content recursively :) in case it's required
                    if (recursive)
                    {
                        DownloadDirectory(client, file.FullName, dir.FullName);
                    }
                }
            }
        }

        public void DownloadFile(SftpClient client, SftpFile file, string directory)
        {
            Console.WriteLine("Downloading {0}", file.FullName);

            using (Stream fileStream = File.OpenWrite(Path.Combine(directory, file.Name)))
            {
                client.DownloadFile(file.FullName, fileStream);
            }
        }

        public string PingChecker(string Host)
        {
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(Host);
            if (pingReply.Status.ToString().Equals("Success"))
            {
                return pingReply.Status.ToString();
            }
            else
            {
                int count = 3;
                while (!pingReply.Status.ToString().Equals("Success") && count > 0)
                {
                    Console.WriteLine(pingReply.Status.ToString());
                    pingReply = ping.Send(Host);
                    count--;
                    Console.WriteLine("Tring Again");
                }
                return pingReply.Status.ToString();
            }
        }
        public void ExecuteScript(string hostip, string machineuser, string pass, string script)
        {
            try
            {

                using (var client = new SshClient(hostip, machineuser, pass))
                {
                    client.Connect();
                    Console.WriteLine("Script Running.....");
                    var cmd = client.RunCommand("source ~/.bash_profile;" + script + ">$IRIS_HOME/LogParsing.sh;dos2unix $IRIS_HOME/LogParsing.sh;sh $IRIS_HOME/LogParsing.sh");
                    client.Disconnect();
                    Console.WriteLine("Result: \n" + cmd.Result);
                    Console.WriteLine("Script End");
                    var error = cmd.Error;
                    string textfilepath = "ErrorLog.txt";
                    File.Delete(textfilepath);
                    if (!File.Exists(textfilepath))
                    {
                        File.Create(textfilepath).Dispose();
                        using (StreamWriter w = File.AppendText(textfilepath))
                        {
                            w.WriteLine(error);
                            w.Close();
                        }
                    }
                    else if (File.Exists(textfilepath))
                    {
                        using (StreamWriter w = File.AppendText(textfilepath))
                        {
                            w.WriteLine(error);
                            w.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("unable to run script :" + ex.Message);
            }
        }

        public void ExecuteFile(string hostip, string machineuser, string pass, string pathtoexecutefile, string argument, string filename)
        {
            try
            {

                using (var client = new SshClient(hostip, machineuser, pass))
                {
                    client.Connect();
                    Console.WriteLine("Script Running.....");
                    var cmd = client.RunCommand("source ~/.bash_profile; dos2unix " + pathtoexecutefile + filename + "; sh " + pathtoexecutefile + filename + argument);
                    client.Disconnect();
                    Console.WriteLine("Result: \n" + cmd.Result);
                    Console.WriteLine("Script End");
                    var error = cmd.Error;
                    string textfilepath = "\\ErrorLog.txt";
                    File.Delete(textfilepath);
                    if (!File.Exists(textfilepath))
                    {
                        File.Create(textfilepath).Dispose();
                        using (StreamWriter w = File.AppendText(textfilepath))
                        {
                            w.WriteLine(error);
                            w.Close();
                        }
                    }
                    else if (File.Exists(textfilepath))
                    {
                        using (StreamWriter w = File.AppendText(textfilepath))
                        {
                            w.WriteLine(error);
                            w.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("unable to run script :" + ex.Message);
            }
        }
    }
}
