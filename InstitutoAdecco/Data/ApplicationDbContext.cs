// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstitutoAdecco.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<InstitutoAdecco.Models.ApplicationUser> ApplicationUser { get; set; }
        public DbSet<InstitutoAdecco.Models.Attachment> Attachment { get; set; }
        public DbSet<InstitutoAdecco.Models.Tag> Tag { get; set; }
        public DbSet<InstitutoAdecco.Models.Category> Category { get; set; }
        public DbSet<InstitutoAdecco.Models.Post> Post { get; set; }
    }
}
