using System;
using System.Collections.Generic;
using System.Text;
using PCIWeb.Tools;
using System.Data;
using System.Collections;
using PCIWeb;
using System.IO;
using System.Web;
using System.Net;
using aejw.Network;
using FileSrvLib;

namespace PYV.WebFileBrowser._ui.Home
{
    [AllCanCall]
    class Index
    {
        protected string UploadFolder = @"";
        string pbfilesrv = "http://www.pyv.com.vn/pbfilesrv/";
        string ShareUser = "";
        string SharePwd = "";
        string FolderName = "";
        string NeedLogin;
        string Name;
        Dictionary<string,object> Setting;//add by TrieuPhan 20170117

        string _fullPath;
        ArrayList _allowFiles;

        /*
        public bool MappingNetWorkNew()
        {
            NetworkDrive oNetDrive = new aejw.Network.NetworkDrive();
            try
            {
                oNetDrive.ShareName = UploadFolder;
                oNetDrive.UnMapDrive();
                oNetDrive.MapDrive(ShareUser, SharePwd);
            }
            catch (Exception ex)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(UploadFolder);
                    if (dir.Exists)
                    {
                        return true;
                    }
                }
                catch
                {
                }

                Tool.Error("Error connection", string.Format("MappingInfo: {0} UserName:{1} Pwd:{2}", UploadFolder, ShareUser, SharePwd));
                return false;
            }
            oNetDrive = null;

            return true;
        }
        */
        
        public bool MappingNetWork()
        {
            
            //NetworkPath.NetworkPath net = new NetworkPath.NetworkPath();
            //try
            //{
            //    net.Force = true;
            //    net.ShareName = UploadFolder;
            //    try
            //    {
            //        net.UnMapDrive();
            //    }
            //    catch (Exception e) { }
            //    net.MapDrive(ShareUser, SharePwd, false);
            //}
            //catch(Exception ex)
            //{
            //    try
            //    {
            //        DirectoryInfo dir = new DirectoryInfo(UploadFolder);
            //        if (dir.Exists)
            //        {
            //            return true;
            //        }
            //    }
            //    catch
            //    {
            //    }

            //    Tool.Error("Error connection" + ex.Message, string.Format("MappingInfo: {0} UserName:{1} Pwd:{2}", UploadFolder, ShareUser, SharePwd));
            //    return false;
            //}
            return true;

        }

        public string GetFolderName()
        {
            return FolderName;
        }

        public string GetNameSystem()
        {
            return Name;
        }
        //add by TrieuPhan 20170117
        public Dictionary<string,object> GetSetting()
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            if (Setting != null)
            {
                foreach (KeyValuePair<string, object> entry in Setting)
                {
                    item.Add(entry.Key.ToLower(), entry.Value);
                }
            }
            
            return item;
        }
        //end 20170117

        public void CheckPermission()
        {
            if (NeedLogin == "true")
            {
                try
                {
                    AuthenticateHelper.Instance.User["UserID"].ToString();
                }
                catch
                {
                    throw new PCINeedLoginException();
                }
            }

            //can edit
            //if (NeedLogin=="true" && AuthenticateHelper.Instance.User["UserID"]==null)
                //throw new PCINeedLoginException();
        }

        List<Dictionary<string, object>> getDirectories(string path, string parentPath)
        {
            List<Dictionary<string, object>> ret = null;
            string[] dirs = Directory.GetDirectories(path);
            
            //sort dirs -> result tree folder sorted
            int length = dirs.Length, i, j;
            string temp;
            for (i = 0; i < length - 1; i++)
            {
                for (j = i + 1; j < length; j++)
                {
                    if (dirs[i].CompareTo(dirs[j]) > 0)
                    {
                        temp = dirs[i];
                        dirs[i] = dirs[j];
                        dirs[j] = temp;
                    }
                }
            }

            if (dirs != null && dirs.Length > 0)
            {
                ret = new List<Dictionary<string, object>>();
                foreach (string dir in dirs)
                {
                    DirectoryInfo directory = new DirectoryInfo(dir);
                    ret.Add(getItem(directory, parentPath));
                }
            }
            return ret;
        }

        //string getRelPath(string fullname,bool isFolder)
        //{
        //    string root = ("/" + this._docDirectory + "/").ToUpper();
        //    fullname = fullname.Replace("\\", "/");
        //    int index = fullname.ToUpper().IndexOf(root);
        //    if (index >= 0)
        //    {
        //        if(isFolder)
        //            return fullname.Substring(index + root.Length);
        //        else
        //            return fullname.Substring(0, fullname.LastIndexOf("/")).Substring(index + root.Length);
        //    }
        //    return "";
        //}

        Dictionary<string, object> getItem(FileInfo file, string path, string keyword)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["text"] = file.Name;
            //item["key"] = file.FullName.Substring(UploadFolder.Length + 1);
            item["key"] = (path != "") ? (path + "\\" + file.Name) : file.Name;
            item["kind"] = "file";
            item["modify"] = file.LastWriteTime.ToString("yyyy/MM/dd HH:mm");
            try
            {
                int size = (int)file.Length / 1024 + 1;//KB 
                item["size"] = size;
            }
            catch { }
            return item;
        }

        Dictionary<string, object> getItem(DirectoryInfo directory, string parentPath)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            item["text"] = directory.Name;
            //item["key"] = directory.FullName.Substring(UploadFolder.Length + 1);
            item["key"] = parentPath != "" ? (parentPath + "\\" + directory.Name) : directory.Name;
            item["kind"] = "folder";
            item["modify"] = directory.LastWriteTime.ToString("yyyy/MM/dd HH:mm");
            return item;
        }

        int CountDir(string path)
        {
            string[] listDir = Directory.GetDirectories(path);
            return listDir.Length;
        }

        //int getFilesCount(DirectoryInfo dir)
        //{
        //    FileInfo[] files = dir.GetFiles();
        //    int i = 0;
        //    if (files != null && files.Length > 0)
        //    {
        //        foreach (FileInfo file in files)
        //        {

        //            if (file.Extension != null && file.Extension.Length > 0 && this._allowFiles.IndexOf(file.Extension.Substring(1).ToLower()) >= 0)
        //                i++;
        //        }
        //    }
        //    return i;
        //}

        public List<Dictionary<string, object>> GetFiles(Dictionary<string, object> args)
        {
            CheckPermission();
            try
            {
                using (new Impersonation(UploadFolder, ShareUser, SharePwd))
                {
                    List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();

                    //string path = UploadFolder + "\\" + args["path"].ToString();
                    string path = UploadFolder;
                    if (args["path"].ToString() != "")
                        path += "\\" + args["path"].ToString();
                    string keyword = args["keyword"] == null ? null : args["keyword"].ToString();

                    DirectoryInfo dir = new DirectoryInfo(rootPath + path);

                    DirectoryInfo[] dirs;
                    if (keyword != null)
                        dirs = dir.GetDirectories("*" + keyword + "*", SearchOption.AllDirectories);
                    else
                        dirs = dir.GetDirectories();
                    if (dirs != null && dirs.Length > 0)
                    {
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            ret.Add(getItem(subdir, args["path"].ToString()));
                        }
                    }

                    #region "Edit by TrieuPhan 20170117"
                    //FileInfo[] files;                    
                    //if (keyword != null)
                    //    files = dir.GetFiles("*" + keyword + "*", SearchOption.AllDirectories);//(*)
                    //else
                    //    files = dir.GetFiles();
                    //if (files != null && files.Length > 0)
                    //{
                    //    foreach (FileInfo file in files)
                    //    {
                    //        if (file.Extension != null && file.Extension.Length > 0 && this._allowFiles.IndexOf(file.Extension.Substring(1).ToLower()) >= 0)
                    //        check again 
                    //        if (keyword != null && keyword != "")
                    //        {
                    //            string keywordTmp = keyword.Replace("*", "");
                    //            if (file.Name.ToUpper().Contains(keywordTmp.ToUpper()))
                    //            {
                    //                string pathFile = "....";
                    //                try
                    //                {
                    //                    pathFile = file.Directory.ToString().Substring((UploadFolder + "\\").Length);
                    //                }
                    //                catch (Exception e)
                    //                {
                    //                    /*remove this in result
                    //                    because not get key so open is error
                    //                    edit: line code getAllFile(*): nen goi de quy tim tung file con cua tung thu muc con
                    //                    khong dung pt cua c#
                    //                     * */
                    //                    continue;
                    //                }
                    //                ret.Add(getItem(file, pathFile/*args["path"].ToString()*/, keywordTmp));
                    //            }
                    //        }
                    //        else
                    //        {
                    //            ret.Add(getItem(file, args["path"].ToString(), keyword));
                    //        }
                    //    }
                    //}
                    FileInfo[] files;
                    Dictionary<string, object> setting = new Dictionary<string, object>();
                    if (args.ContainsKey("setting") && args["setting"] != null)
                        setting = (Dictionary<string, object>)args["setting"];

                    string showfiles = "Y";
                    if (setting.ContainsKey("showfiles") && setting["showfiles"] != null)
                        showfiles = setting["showfiles"].ToString();

                    if (keyword != null)
                    {
                        files = dir.GetFiles("*" + keyword + "*", SearchOption.AllDirectories);
                        if (files != null && files.Length > 0)
                        {
                            foreach (FileInfo file in files)
                            {                                
                                //check again 
                                if (keyword != null && keyword != "")
                                {
                                    string keywordTmp = keyword.Replace("*", "");
                                    if (file.Name.ToUpper().Contains(keywordTmp.ToUpper()))
                                    {
                                        string pathFile = "....";
                                        try
                                        {
                                            pathFile = file.Directory.ToString().Substring((UploadFolder + "\\").Length);
                                        }
                                        catch (Exception e)
                                        {
                                            continue;
                                        }
                                        ret.Add(getItem(file, pathFile, keywordTmp));
                                    }
                                }
                                else
                                {
                                    ret.Add(getItem(file, args["path"].ToString(), keyword));
                                }
                            }
                        }
                    }
                    else if (showfiles == "Y")
                    {
                        files = dir.GetFiles();
                        if (files != null && files.Length > 0)
                        {
                            foreach (FileInfo file in files)
                            {
                                //check again 
                                if (keyword != null && keyword != "")
                                {
                                    string keywordTmp = keyword.Replace("*", "");
                                    if (file.Name.ToUpper().Contains(keywordTmp.ToUpper()))
                                    {
                                        string pathFile = "....";
                                        try
                                        {
                                            pathFile = file.Directory.ToString().Substring((UploadFolder + "\\").Length);
                                        }
                                        catch (Exception e)
                                        {
                                            continue;
                                        }
                                        ret.Add(getItem(file, pathFile, keywordTmp));
                                    }
                                }
                                else
                                {
                                    ret.Add(getItem(file, args["path"].ToString(), keyword));
                                }
                            }
                        }
                    }
                    #endregion
                    return ret;
                }
            }
            catch (Exception e)
            {
                throw new PCIBusException(e.Message);
            }
        }

        string rootPath
        {
            get
            {
                //return HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath + "/" + this._docDirectory);
                //return HttpContext.Current.Server.MapPath(_fullPath);
                return "";

            }
        }

        Dictionary<string, object> getLastWrite(string path)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);

            DateTime dt = DateTime.MinValue;
            FileInfo fi = null;
            foreach (FileInfo file in files)
            {
                if (file.LastWriteTime > dt)
                {
                    dt = file.LastWriteTime;
                    fi = file;
                }
            }
            //ret.Add("LastWriteTime", dir.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"));
            ret.Add("LastWriteTime", dt.ToString("yyyy/MM/dd HH:mm:ss "));
            ret.Add("LastWriteFile", fi != null ? fi.FullName.Substring(path.Length + 1) : "");
            return ret;
        }

        public List<Dictionary<string, object>> AllDirs(string path)
        {
            CheckPermission();
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string dirPath = UploadFolder;
                if (path != "")
                    dirPath += "\\" + path;
                //path = UploadFolder + "\\" + path;
                List<Dictionary<string, object>> ret = getDirectories(dirPath, path);
                return ret;
            }
        }

        public void CreateFolder(string path)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string fullPath = Path.Combine(UploadFolder, path);
                try
                {
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);
                    else
                        throw new PCIBusException("Name folder is existed or invalid!");
                }
                catch (Exception ex)
                {
                    throw new PCIBusException(ex.Message);
                }
            }
        }

        public void Rename(string oldName, string newName, int user_id, string link, bool overwrite)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string fullOldName = Path.Combine(UploadFolder, oldName);
                string fullNewName = Path.Combine(UploadFolder, newName);
                oldName = fullOldName;
                newName = fullNewName;

                try
                {
                    //item la file
                    if (File.Exists(fullOldName))
                    {
                        if (File.Exists(fullNewName) && overwrite)
                            //File.Delete(fullNewName);
                            Delete(newName, user_id, link);
                        File.Move(fullOldName, fullNewName);
                    }

                    else
                    {
                        if (Directory.Exists(fullNewName) && overwrite)
                            //Directory.Delete(fullNewName, true);
                            Delete(newName, user_id, link);
                        Directory.Move(fullOldName, fullNewName);
                    }
                    Tool.Info("Rename OK", "Source", oldName, "Target", newName, "User_id", user_id, "link", link);
                }
                catch (Exception e)
                {
                    throw new PCIBusException(e.Message);
                }
            }
        }

        string Delete(string path, int user_id, string link)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                path = Path.Combine(UploadFolder, path);
                string ex = null;
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    else
                        Directory.Delete(path, true);
                    Tool.Info("Delete OK", "Path", path, "User_id", user_id, "link", link);
                }
                catch (Exception e)
                {
                    ex = e.Message;
                }
                return ex;
            }
        }

        public void Delete(ArrayList path, int user_id, string link)
        {
            string ex = null;
            string mess = null;
            for (int i = 0; i < path.Count; i++)
            {
                string iPath = path[i].ToString();
                ex = Delete(iPath, user_id, link);
                if (ex != null)
                {
                    mess = ex;
                }
            }
            if (mess != null)
            {
                throw new PCIBusException(mess);
            }

        }

        string CopyFolder(string source, string target, int user_id, string link)
        {
            string fullSource = Path.Combine(UploadFolder, source);
            string fullTarget = Path.Combine(UploadFolder, target);
            string name = Path.GetFileName(fullSource);
            string dest = Path.Combine(fullTarget, name);
            string mess = null;

            try
            {
                //dest da ton tai
                if (Directory.Exists(dest))
                    //Directory.Delete(dest, true);
                    Delete(dest, user_id, link);
                Directory.CreateDirectory(dest);
                string[] files = Directory.GetFiles(fullSource);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string fileDest = Path.Combine(dest, fileName);
                    File.Copy(file, fileDest, true);
                }

                string[] dirs = Directory.GetDirectories(fullSource);
                foreach (string dir in dirs)
                {
                    CopyFolder(dir, dest, user_id, link);
                }
            }
            catch (Exception e)
            {
                mess = e.Message;
            }
            return mess;
        }

        string Copy(string source, string target, int user_id, string link)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string fullSource = Path.Combine(UploadFolder, source);
                string fullTarget = Path.Combine(UploadFolder, target);
                string name = Path.GetFileName(fullSource);
                string dest = Path.Combine(fullTarget, name);
                string ex = null;

                try
                {
                    if (File.Exists(fullSource))
                        File.Copy(fullSource, Path.Combine(fullTarget, name), true);
                    else
                        CopyFolder(source, /*Path.Combine(target, name)*/target, user_id, link);
                    Tool.Info("Copy OK", "Source", source, "Target", target, "User_id", user_id, "link", link);
                }
                catch (Exception e)
                {
                    ex = e.Message;
                }
                return ex;
            }
        }

        public Dictionary<string, ArrayList> Copy(ArrayList source, string target, int user_id, string link, bool overwrite)
        {

            Dictionary<string, ArrayList> args = new Dictionary<string, ArrayList>();
            ArrayList arr = new ArrayList();
            ArrayList existed = new ArrayList();
            string name;
            string fullSource;
            string fullTarget = Path.Combine(UploadFolder, target);
            string dest;
            string ex = null;
            string mess = null;
            for (int i = 0; i < source.Count; i++)
            {
                //string source = source[i].ToString();
                fullSource = Path.Combine(UploadFolder, source[i].ToString());
                name = Path.GetFileName(fullSource);
                dest = Path.Combine(fullTarget, name);

                if (!overwrite && CheckExist(dest))
                {
                    existed.Add(source[i].ToString());
                }
                else
                {
                    ex = Copy(source[i].ToString(), target, user_id, link);
                    if (ex != null)
                        mess = ex;
                }
            }
            if (mess != null)
            {
                throw new PCIBusException(mess);
            }
            args.Add("existed", existed);
            return args;
        }

        string Cut(string source, string target, int user_id, string link)
        {
            //string fullSource = Path.Combine(UploadFolder, source);
            //try
            //{
            //    string name = Path.GetFileName(fullSource);
            //    string dest = Path.Combine(target, name);
            //    //result = Rename(source, dest, user_id, link, true);             
            //    Tool.Info("Cut OK", "Source", source, "Target", target, "User_id", user_id, "link", link);
            //}
            //catch
            //{
            //    result = false;
            //}
            //return result;
            return Move(source, target, user_id, link);
        }

        string Move(string source, string target, int user_id, string link)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string rand = "AX732Xy6h1z";
                string ex = null;
                string fullSource = Path.Combine(UploadFolder, source);
                string fullTarget = Path.Combine(UploadFolder, target);
                string name = Path.GetFileName(fullSource);
                string dest = Path.Combine(fullTarget, name);
                string destRand = Path.Combine(fullTarget, rand);
                try
                {
                    if (File.Exists(fullSource))
                    {
                        //xu ly source la file
                        if (CheckExist(Path.Combine(target, name)))
                        {
                            File.Move(fullSource, destRand);
                            File.Delete(dest);
                            File.Move(destRand, dest);
                        }
                        else
                        {
                            File.Move(fullSource, dest);
                        }
                    }
                    else
                    {
                        //xu ly source la dir
                        if (CheckExist(Path.Combine(target, name)))
                        {
                            Directory.Move(fullSource, destRand);
                            Directory.Delete(dest, true);
                            Directory.Move(destRand, dest);
                        }
                        else
                        {
                            Directory.Move(fullSource, dest);
                        }
                    }
                    Tool.Info("Cut OK", "Source", source, "Target", target, "User_id", user_id, "link", link);
                }
                catch (Exception e)
                {
                    ex = e.Message;
                    //rollback
                    if (CheckExist(Path.Combine(target, rand)))
                    {
                        if (File.Exists(Path.Combine(target, rand)))
                        {
                            File.Move(destRand, fullSource);
                        }
                        else
                        {
                            Directory.Move(destRand, fullSource);
                        }
                    }
                }
                return ex;
            }
        }

        public Dictionary<string, ArrayList> Cut(ArrayList source, string target, int user_id, string link, bool overwrite)
        {
            string ex = null;
            string mess = null;
            ArrayList arr = new ArrayList();
            ArrayList existed = new ArrayList();
            Dictionary<string, ArrayList> args = new Dictionary<string, ArrayList>();
            string dest;
            string name;

            for (int i = 0, len = source.Count; i < len; i++)
            {
                name = Path.GetFileName(source[i].ToString());
                dest = Path.Combine(target, name);
                if (!overwrite && CheckExist(dest))
                {
                    existed.Add(source[i].ToString());
                }
                else
                {
                    ex = Cut(source[i].ToString(), target, user_id, link);
                    if (ex != null)
                    {
                        mess = ex;
                    }
                }
            }
            if (mess != null)
            {
                throw new PCIBusException(mess);
            }
            args.Add("existed", existed);
            return args;
        }

        bool CheckExist(string path)
        {
            string fullPath = Path.Combine(UploadFolder, path);
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return true;
                return false;
            }
        }

        //checkIsAdmin
        bool IsAdmin(Dictionary<string, object> args)
        {
            //vao superUser kiem tra co userId + menu_id nay ko
            //neu co: admin ->true
            //ngc lai: user ->false
            DataRow dr = Tool.ToRow(DBHelper.Instance.Query("PYV.WebFileBrowser.AddUser.checkIsAdmin", args));
            if (dr != null)
                return true;
            return false;
        }

        //get menu_id of Sys Web
        string GetMenuId(Dictionary<string, object> args)
        {
            string menu_id="";
            DataRow dr = Tool.ToRow(DBHelper.Instance.Query("PYV.WebFileBrowser.AddUser.getMenuId", args));
            if (dr != null)
                menu_id = dr[0].ToString();
            return menu_id;
        }

        public bool CheckIsAdmin(Dictionary<string, object> args)
        {
            string menu_id = GetMenuId(args);
            args.Add("menu_id", menu_id);
            return IsAdmin(args);
        }

        #region "Phan upload"
        public string SaveUploadFile(Dictionary<string, object> args)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                HttpFileCollection uploadFile = HttpContext.Current.Request.Files;
                if (uploadFile != null && uploadFile.Count > 0)
                {
                    for (int i = 0; i < uploadFile.Count; i++)
                    {
                        HttpPostedFile uploadfile = uploadFile[i];

                        if (uploadfile.ContentLength > 0)
                        {
                            string filename = uploadfile.FileName;
                            uploadfile.SaveAs(UploadFolder + args["path"].ToString() + filename);
                            //uploadfile.SaveAs(UploadFolder + filename);
                        }

                    }

                }
                return "Upload file succecss";
            }
            /*
            try
            {
                if (MappingNetWork())
                {
                    HttpPostedFile uploadFile = HttpContext.Current.Request.Files[args["FILE_ID_UPLOAD"].ToString()];
                    if (uploadFile != null && uploadFile.ContentLength > 0)
                    {
                        for (int i = 0; i < args["Filename"].ToString().Split('|').Length - 1; i++)
                        {
                            string a = UploadFolder + args["path"].ToString() + args["Filename"].ToString().Split('|')[i].ToString();
                            uploadFile.SaveAs(UploadFolder + args["path"].ToString() + args["Filename"].ToString().Split('|')[i].ToString());
                        }
                    }
                    return "Upload file succecss";
                }
                else
                    return "Map Network Drive Error !";
            }
            catch (Exception e)
            {
                return "Upload file Error";
            }
             */
        }
        public Dictionary<string, object> QueryAttachByID(Dictionary<string, object> args)
        {
            
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                string filePath = args["PATH"].ToString();
                HttpResponse response = HttpContext.Current.Response;
                response.Clear();
                response.ClearContent();
                response.ClearHeaders();
                response.Buffer = true;
                response.AddHeader("Content-Disposition", "attachment;filename=" + filePath.Split('\\')[filePath.Split('\\').Length - 1] + "");
                byte[] data = System.IO.File.ReadAllBytes(UploadFolder + "\\" + filePath);
                response.BinaryWrite(data);
                response.End();
            }
            return null;
        }
        public string OpenFile(string path)
        {
            string text = File.ReadAllText(UploadFolder + "\\" + path, Encoding.UTF8);
            return text;
        }
        public string DownLoadFile(Dictionary<string, object> args)
        {
           
            if (!MappingNetWork())
                throw new PCIBusException("Map Network Drive Error !");
            return UploadFolder;
        }

        public Dictionary<string, object> ViewFile(Dictionary<string, object> args)
        {
            using (new Impersonation(UploadFolder, ShareUser, SharePwd))
            {
                return Tool.ToDic(
                            "Name", args["PATH"].ToString().Split('\\')[args["PATH"].ToString().Split('\\').Length - 1].ToString()
                            , "Format", (args["PATH"].ToString().Split('.')[args["PATH"].ToString().Split('.').Length - 1]).ToString()
                            , "Bytes", System.IO.File.ReadAllBytes(UploadFolder + "\\" + args["PATH"].ToString())
                        );
            }
        }
        #endregion

        private string _n;

        public string n
        {
            get { return _n; }
            set { _n = value; }
        }
    }
}
