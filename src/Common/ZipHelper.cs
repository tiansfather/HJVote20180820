using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Common
{
    public class ZipHelper
    {
        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="targetFile">解压文件路径</param>
        /// <param name="zipFile">解压文件后路径</param>
        public static List<string> Decompression(string targetFile, string zipFile)
        {
            var result = new List<string>();
            using (var archive = ArchiveFactory.Open(targetFile))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        result.Add(entry.Key);
                        entry.WriteToDirectory(zipFile);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="fromFileDirectory">压缩文件夹路径</param>
        /// <param name="outFilePath">压缩后路径</param>
        public static void Zips(string fromFileDirectory, string outFilePath)
        {
            //解决中文乱码问题
            SharpCompress.Common.ArchiveEncoding ArchiveEncoding = new SharpCompress.Common.ArchiveEncoding();
            ArchiveEncoding.Default = Encoding.GetEncoding("utf-8");
            SharpCompress.Writers.WriterOptions options = new SharpCompress.Writers.WriterOptions(CompressionType.Deflate);
            options.ArchiveEncoding = ArchiveEncoding;

            using (var archive = ZipArchive.Create())
            {
                archive.AddAllFromDirectory(fromFileDirectory);
                using (var zip = File.OpenWrite(outFilePath))
                    archive.SaveTo(zip, options);
            }
        }
        

        public static IEnumerable<string> GetFileNames(string targetFile)
        {
            using (var archive = ArchiveFactory.Open(targetFile))
            {
                return archive.Entries.Select(o => o.Key);
            }
        }
    }
}
