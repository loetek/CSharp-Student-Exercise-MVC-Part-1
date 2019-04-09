using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExerciseMVC.Models;
using StudentExerciseMVC.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using StudentExerciseMVC.Models.ViewModels;

namespace StudentExerciseMVC.Controllers
{
    public class CohortsController : Controller
    {
       
            private readonly IConfiguration _configuration;

            public CohortsController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public SqlConnection Connection
            {
                get
                {
                    return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                }
            }

            // GET: Cohorts
            public ActionResult Index()
            {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            SELECT c.Id,
                                                   c.[Name]
                                            FROM Cohort c";
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Cohort> cohorts = new List<Cohort>();
                        while (reader.Read())
                        {
                            Cohort cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            };

                            cohorts.Add(cohort);
                        }

                        reader.Close();

                        return View(cohorts);
                    }
                }
            }

            // GET: Cohorts/Details/5
            public ActionResult Details(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"select c.id, c.[name], 
                                               s.id AS StudentId, s.FirstName AS StudentFirstname, 
                                               s.LastName AS StudentLastName, 
                                               s.SlackHandle AS StudentSlackHandle,
                                               i.id AS InstructorId, i.FirstName AS InstructorFirstName,
                                               i.LastName AS InstructorLastName, 
                                               i.SlackHandle AS InstructorSlackHandle
                                          from cohort c 
                                               left join student s on c.id = s.cohortid
                                               left join Instructor i on c.id = i.CohortId
                                         where c.id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Cohort cohort = null;
                        while (reader.Read())
                        {
                            if (cohort == null)
                            {
                                cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name"))
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                            {
                                int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                                if (!cohort.Students.Any(s => s.Id == studentId))
                                {
                                    Student student = new Student
                                    {
                                        Id = studentId,
                                        FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                        CohortId = cohort.Id
                                    };
                                    cohort.Students.Add(student);
                                }
                            }


                            if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                            {
                                int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                                if (!cohort.Instructors.Any(i => i.Id == instructorId))
                                {
                                    Instructor instructor = new Instructor
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                        FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                        CohortId = cohort.Id
                                    };

                                    cohort.Instructors.Add(instructor);
                                }
                            }
                        }


                        reader.Close();
                        return View(cohort);
                    }
                }
            }

            // GET: Cohorts/Create
            public ActionResult Create()
            {
         
            return View();

            }

            // POST: Cohorts/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(CohortCreateViewModel viewModel)
            {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO cohort ([Name])
                                             VALUES (@cohortName)";
                        cmd.Parameters.Add(new SqlParameter("@cohortName", viewModel.Cohort.Name));
                     

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                viewModel.Cohorts = GetAllCohorts();
                return View(viewModel);
            }
        }

        // GET: Cohorts/Edit/5
        public ActionResult Edit(int id)
        {
            Cohort cohort = GetCohortById(id);
            if (cohort == null)
            {
                return NotFound();
            }
            return  View();
        }

        // POST: Cohorts/Edit/5
        [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(int id, CohortEditViewModel viewModel)
            {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort 
                                           SET [Name] = @cohortName, 
                                           WHERE id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@cohortName", viewModel.Cohort.Name));
                       
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                viewModel.Cohorts = GetAllCohorts();
                return View(viewModel);
            }
        }

            // GET: Cohorts/Delete/5
            public ActionResult Delete(int id)
            {
                return View();
            }

            // POST: Cohorts/Delete/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Delete(int id, IFormCollection collection)
            {
                try
                {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM cohort WHERE id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }



            private Cohort GetCohortById(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT Id AS CohortId,
                                                   [Name] AS CohortName
                                         FROM Cohort
                                         WHERE  Id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Cohort cohort = null;

                        if (reader.Read())
                        {
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            };
                        
                        }

                        reader.Close();

                        return cohort;
                    }
                }

            }






        private List<Cohort> GetAllCohorts()
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT id, name from Cohort;";
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Cohort> cohorts = new List<Cohort>();

                        while (reader.Read())
                        {
                            cohorts.Add(new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("name"))
                            });
                        }
                        reader.Close();

                        return cohorts;
                    }
                }

            }
    }
    }
