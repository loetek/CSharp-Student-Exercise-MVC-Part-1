using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentExerciseMVC.Models.ViewModels
{
    public class CohortCreateViewModel
    {
      
        public CohortCreateViewModel()
        {
            Cohorts = new List<Cohort>();
            Cohort = new Cohort();
        }

        public CohortCreateViewModel(string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT id, name from Cohort;";
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohorts.Add(new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("name"))
                        });
                    }
                    reader.Close();
                }
            }
        }


        public Instructor Instructor { get; set; }
        public Student Student { get; set; }
        public List<Cohort> Cohorts { get; set; }
        public Cohort Cohort { get; private set; }

        public List<SelectListItem> CohortOptions
        {
            get
            {
                return Cohorts.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            }
        }
    }
    }

