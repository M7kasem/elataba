using ElAtaba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Elattba.InfraStructure.Data
{
    public class El3atbaDbContext:DbContext
    {
        public El3atbaDbContext(DbContextOptions<El3atbaDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Governorate> Governorates => Set<Governorate>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<PricingTier> PricingTiers => Set<PricingTier>();
        public DbSet<Offer> Offers => Set<Offer>();
        public DbSet<OfferProduct> OfferProducts => Set<OfferProduct>();
        public DbSet<Carrier> Carriers => Set<Carrier>();
        public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Message> Messages => Set<Message>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
}}
