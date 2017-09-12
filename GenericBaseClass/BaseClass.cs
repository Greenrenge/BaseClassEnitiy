using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BaseClassEnitiy.Models
{
    //https://cpratt.co/generic-entity-base-class/
    public interface IModifiableEntity
    {
        string Name { get; set; }
    }
    public interface IActiveFlagEntity
    {
        string Active { get; set; }
    }
    public interface IEntity : IModifiableEntity
    {
        object Id { get; }
        DateTime CreatedDate { get; set; }
        DateTime? UpdatedDate { get; set; }
        string CreatedBy { get; set; }//http://www.jigar.net/articles/viewhtmlcontent344.aspx
        string UpdatedBy { get; set; }
        byte[] Version { get; set; }
    }

    public interface IEntity<T> : IEntity
    {
        new T Id { get; set; }
    }

    public abstract class Entity<T> : IEntity<T>, IActiveFlagEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T Id { get; set; }

        [NotMapped]
        object IEntity.Id
        {
            get { return this.Id; }
        }

        [NotMapped]
        public string Name { get; set; }//ใช้ทำอะไรไม่แน่ใจ เหมือนใช้ประโยชน์กับ viewModel

        private DateTime? createdDate;
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate
        {
            get { return createdDate ?? DateTime.Now; }//change from utc to normal ; will be overrided again when save by overridden method save change of dbcontext
            set { createdDate = value; }
        }

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }


        [NotMapped]
        private string _active;//https://stackoverflow.com/questions/13807406/entity-framework-many-to-many-through-containing-object/13810766#13810766

        [Required]
        public string Active
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_active))
                {
                    _active = "A";
                }
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}