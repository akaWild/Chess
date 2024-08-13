﻿using MatchService.Models;
using Microsoft.EntityFrameworkCore;

namespace MatchService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public required DbSet<Match> Matches { get; set; }
    }
}