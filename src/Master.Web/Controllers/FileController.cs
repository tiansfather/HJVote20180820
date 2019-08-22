using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.UI;
using Abp.Web.Models;
using Master.Controllers;
using Master.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Master.Web.Controllers
{
    public class UploadResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Msg { get; set; }
    }
    [AbpMvcAuthorize]
    public class FileController : MasterControllerBase
    {

        private async Task<UploadResult> UploadFile(IFormFile file,bool isFromAdmin=false)
        {
            string ext = Path.GetExtension(file.FileName);

            DateTime now = DateTime.Now;
            string upload_path = $"{Directory.GetCurrentDirectory()}\\wwwroot\\{(isFromAdmin?"filesadmin":"files")}\\{now.ToString("yyyyMMdd")}";

            Directory.CreateDirectory(upload_path);


            
            var filenameWithOutPath = Guid.NewGuid().ToString() + ext;
            var filename = upload_path + "\\" + filenameWithOutPath;
            //path = "/images/" + now.Year + "/" + now.ToString("MMdd") + "/" + filenamenew;
            //size += file.Length;
            using (FileStream fs = System.IO.File.Create(filename))
            {
                await file.CopyToAsync(fs);
                fs.Flush();

            }
            //虚拟路径
            var virtualPath = $"/{(isFromAdmin ? "filesadmin" : "files")}/{now.ToString("yyyyMMdd")}/{filenameWithOutPath}";

            var result = new UploadResult {Success=true, FilePath = virtualPath, FileName = file.FileName };
            return result;
        }
        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Upload(bool mustPDF=false,bool isFromAdmin=false)
        {
            //throw new UserFriendlyException("Not Available");
            //if(new Random().Next(1, 4) > 2)
            //{
            //    return Json(new UploadResult { Success = false, Msg = "请稍候重试" });
            //}
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    //可以写遍历files
                    var file = files[0];
                    string ext = Path.GetExtension(file.FileName);

                    if (mustPDF && ext.ToLower() != ".pdf")
                    {
                        return Json(new UploadResult { Success = false, Msg = "本系统只接收PDF文件上传" });
                    }
                    var result = await UploadFile(file, isFromAdmin);

                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.StackTrace);
                return Json(new UploadResult { Success = false, Msg = "系统繁忙,请稍候重试" });
            }
            

            throw new UserFriendlyException("未找到上传文件");
        }
        [DontWrapResult]
        public async Task<object> LayEditUpload()
        {
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                //可以写遍历files
                var file = files[0];

                var uploadResult = await UploadFile(file);

                var result = new ResultDto()
                {
                    data = new { src=uploadResult.FilePath,title=uploadResult.FileName}
                };

                return Json(result);
            }

            throw new UserFriendlyException("未找到上传文件");
        }

        public ActionResult DownLoad(string fileName,string filePath)
        {
            return File(filePath, "application/octet-stream", fileName);
        }

        /// <summary>
        /// 通过文件名获了文件的Content-Type
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetFileContentType(string filename)
        {
            //todo:通过文件名获了文件的Content-Type
            var ext = System.IO.Path.GetExtension(filename);
            //var mimeType = MimeMapping.GetMimeMapping(fileName);
            string ContentType = "";
            switch (ext)
            {
                case ".png":
                    ContentType = "image/png";
                    break;
                case ".jpg":
                case ".jpe":
                case ".jpeg":
                    ContentType = "image/jpeg";
                    break;
                default:
                    ContentType = "application/octet-stream";
                    break;
            }
            return ContentType;
        }
    }
}