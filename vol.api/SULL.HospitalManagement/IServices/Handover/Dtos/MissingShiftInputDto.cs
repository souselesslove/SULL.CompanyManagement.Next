using System;
using VOL.Entity.DomainModels.Custom;

namespace SULL.HospitalManagement.IServices.Handover.Dtos
{
    public class MissingShiftInputDto: DtoBase
    {
        public long RelatedId { get; set; }
        /// <summary>
        /// 多个班次用逗号分隔
        /// </summary>
        public string ShiftNames { get; set; }
        public DateTime ShiftTime { get; set; }
    }
}
