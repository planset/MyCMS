using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCMS.Models
{
    public class Post : TableEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime? PublishDate { get; set; }

        public string Status { get; set; }

        public string Categories { get; set; }
        public string Tags { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

        public string WpLink { get; set; }
        public int WpPostId { get; set; }

        /// <summary>
        /// パラメータのないコンストラクタが必ず必要
        /// </summary>
        public Post()
        {
            this.PartitionKey = "POST";
        }
    }
}
