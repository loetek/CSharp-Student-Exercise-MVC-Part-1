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

namespace StudentExerciseMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _configuration;

        public StudentsController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public SqlConnection Connection
        {

            get { return new SqlConnection(_configuration.GetConnectionString("DefaultConnection")); }
        }



        // GET: Students
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id AS StudentId,
                                               s.FirstName,
                                               s.LastName,
                                               s.SlackHandle,
                                               s.CohortId,
                                               c.[Name] AS CohortName
                                          FROM student s
                                               LEFT JOIN Cohort c ON s.CohortId = c.id;";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {

                        Student student = new Student()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };

                        students.Add(student);

                    }

                    reader.Close();
                    return View(students);
                }
            }
        }

        // GET: Students/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, 
                                               s.firstname, 
                                               s.lastname,
                                               s.slackhandle,
                                               s.cohortId, 
                                               c.name AS cohortName
                                        FROM Student s INNER JOIN Cohort c ON s.cohortid = c.id
                                        WHERE s.id = @id;";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                            LastName = reader.GetString(reader.GetOrdinal("lastname")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slackhandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortid")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortid")),
                                Name = reader.GetString(reader.GetOrdinal("cohortName"))
                            }
                        };
                    }

                    reader.Close();
                    return View(student);
                }
            }
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            StudentCreateViewModel viewModel =
                new StudentCreateViewModel(_configuration.GetConnectionString("DefaultConnection"));
            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO student (firstname, lastname, slackhandle, cohortid)
                                             VALUES (@firstname, @lastname, @slackhandle, @cohortid)";

                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackhandle", viewModel.Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", viewModel.Student.CohortId));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }       
            }
            catch
            {
                
                return View();
            }
        }

        // GET: Students/Edit/5
        public ActionResult Edit(int id)
        {
            Student student = GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            //This piece of code keeps the type correct to pass into the edit
            StudentEditViewModel viewModel = new StudentEditViewModel
            {
                Cohorts = GetAllCohorts(),
                Student = student
            };

            return View(viewModel);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentEditViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE student 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slackhandle = @slackhandle, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";

                        cmd.Parameters.Add(new SqlParameter("@firstname", viewModel.Student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", viewModel.Student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackhandle", viewModel.Student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortid", viewModel.Student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View(viewModel);
            }
        }

        // GET: Students/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        private Student GetStudentById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id AS StudentId,
                                               s.FirstName, 
                                               s.LastName, 
                                               s.SlackHandle, 
                                               s.CohortId,
                                               c.Name AS CohortName
                                         FROM Student s LEFT JOIN Cohort c on s.cohortid = c.id
                                         WHERE  s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                            }
                        };
                    }

                    reader.Close();

                    return student;

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