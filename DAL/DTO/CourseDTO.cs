using AutoMapper.Configuration.Annotations;
using BAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAL.DTO
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string ImageUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public int? TotalEnrollment { get; set; }
        public int LectureCount { get; set; }
        public int ContentLength { get; set; }
        public decimal? AverageRating { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public int SpecializationId { get; set; }
        public int MentorId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? SpecializationName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MentorName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MentorProfilePictureUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? TotalRatingCount { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? LecturePendingCount { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? Enrolled { get; set; }


        //public CategoryDTO? Category { get; set; }

    }
}
