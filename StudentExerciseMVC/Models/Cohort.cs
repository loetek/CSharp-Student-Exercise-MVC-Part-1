using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExerciseMVC.Models
{
    public class Cohort
    {
        
            public int Id { get; set; }
            [Required]
            [Display(Name = "Cohort Name")]
            public string Name { get; set; }

            public Instructor Instructor { get; set; }
            public Student Student { get; set; }


            public List<Student> Students { get; set; } = new List<Student>();
            public List<Instructor> Instructors { get; set; } = new List<Instructor>();
        
    }
}
