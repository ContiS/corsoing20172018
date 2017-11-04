﻿using Microsoft.AspNetCore.Mvc;
using ServiceAPI.Dal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ServiceAPI
{
    [Route("api/studentmanagement")]
    public class StudentApiController : Controller
    {
        static readonly object setupLock = new object();
        static readonly SemaphoreSlim parallelism = new SemaphoreSlim(2);
               
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                await parallelism.WaitAsync();

                using (var context = new UniversityDbContext())
                {
                    return Ok(context.Students.ToList());
                }
            }
            finally
            {
                parallelism.Release();
            }
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetStudent([FromQuery]int id)
        {
            using (var context = new UniversityDbContext())
            {
                return Ok(await context.Students.FirstOrDefaultAsync(x => x.Id == id));
            }
        }

        [HttpPut("students")]
        public async Task<IActionResult> CreateStudent([FromBody]Student student)
        {
            using (var context = new UniversityDbContext())
            {
                context.Students.Add(student);

                await context.SaveChangesAsync();

                return Ok();
            }
        }

        [HttpPost("students")]
        public async Task<IActionResult> UpdateStudent([FromBody]Student student)
        {
            using (var context = new UniversityDbContext())
            {
                context.Students.Update(student);
                await context.SaveChangesAsync();
                return Ok();
            }
        }   


        [HttpDelete("students")]
        public async Task<IActionResult> DeleteStudent([FromQuery]int id)
        {
            using (var context = new UniversityDbContext())
            {
                var student = await context.Students.FirstOrDefaultAsync(x => x.Id == id);
                if (student != null)
                {
                    context.Students.Remove(student);
                    await context.SaveChangesAsync();
                }
                return Ok();


            }
        }
    }
}