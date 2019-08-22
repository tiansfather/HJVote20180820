using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master.Domain;

namespace Master
{
    public class FileManager : DomainServiceBase<File, int>, IFileManager
    {
        public string GetFileUrl(int fileId,int w=0,int h=0)
        {
            return $"/File/Thumb?fileid={fileId}&w={w}&h={h}";
        }

        /// <summary>
        /// todo:文件上传方法封装，将上传路径做为参数传递
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<File> UploadFle(IFormFile file)
        {
            DateTime now = DateTime.Now;
            string upload_path = $"{Directory.GetCurrentDirectory()}\\wwwroot\\files\\{now.Year}\\{now.ToString("MM")}\\{now.ToString("dd")}";

            Directory.CreateDirectory(upload_path);

            var filename = file.FileName;
            string ext = Path.GetExtension(filename);
            //var filenamenew = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ext;
            var filenamenew = System.Guid.NewGuid().ToString() + ext;

            filename = upload_path + "\\" + filenamenew;
            //path = "/images/" + now.Year + "/" + now.ToString("MMdd") + "/" + filenamenew;
            //size += file.Length;
            using (FileStream fs = System.IO.File.Create(filename))
            {
                await file.CopyToAsync(fs);
                fs.Flush();

            }
            //虚拟路径
            var virtualPath = $"/files/{now.Year}/{now.ToString("MM")}/{now.ToString("dd")}/{filenamenew}";
            var fileModel = new File()
            {
                TenantId = AbpSession.TenantId.Value,
                FileName = file.FileName,
                FilePath = virtualPath,
                FileSize = Convert.ToDecimal(file.Length) / 1024
            };

            await InsertAndGetIdAsync(fileModel);

            return fileModel;
        }
    }
}
