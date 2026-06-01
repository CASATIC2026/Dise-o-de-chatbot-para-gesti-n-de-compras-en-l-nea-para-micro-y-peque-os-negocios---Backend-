using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Core.Entities;

namespace Shared.Core.Mappings
{
       /// <summary>
       /// Configures the entity mapping for the <see cref="Categoria"/> class.
       /// This includes table name, primary key, property constraints, and relationships.
       /// </summary>
       public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
       {
              /// <summary>
              /// Configures the entity of type <see cref="Categoria"/>.
              /// </summary>
              /// <param name="builder">The builder to be used to configure the entity type.</param>
              public void Configure(EntityTypeBuilder<Categoria> builder)
              {                     
                     // Sets the table name for the Categoria entity in the database.
                     builder.ToTable("Categorias");
                     
                     // Configures the 'Id' property as the primary key for the Categoria entity.
                     builder.HasKey(c => c.Id);
                     
                     // Configures the 'Nombre' property: it is required and has a maximum length of 50 characters.
                     builder.Property(c => c.Nombre)
                            .IsRequired()
                            .HasMaxLength(50);

                     // Configures the 'Descripcion' property with a maximum length of 200 characters.
                     builder.Property(c => c.Descripcion)
                            .HasMaxLength(200);
                     
                     // Configures the 'CreadoEn' property to automatically set its default value
                     // to the current timestamp upon creation in PostgreSQL.
                     builder.Property(c => c.CreadoEn)
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");

                     // Configures the 'ActualizadoEn' property to automatically set its default value
                     // to the current timestamp upon update in PostgreSQL.
                     builder.Property(c => c.ActualizadoEn)
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");
                     
                     // Configures a one-to-many relationship: a Categoria can have many Productos.
                     // The foreign key 'CategoriaId' in the Producto entity links back to Categoria.
                     builder.HasMany(c => c.Productos)
                            .WithOne(p => p.Categoria)
                            .HasForeignKey(p => p.CategoriaId);
              }

       }
}