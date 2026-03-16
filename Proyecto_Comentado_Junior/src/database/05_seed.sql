SET search_path TO ecommerce;

INSERT INTO categorias (nombre, descripcion)
VALUES 
('Electrónica', 'Dispositivos electrónicos'),
('Ropa', 'Prendas de vestir');

INSERT INTO usuarios (nombre, contrasena_hash, email, rol)
VALUES
('Admin', 'hash123', 'admin@email.com', 'ADMIN');

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'ecommerce';

INSERT INTO ecommerce.productos (
    nombre, precio, stock, id_categoria
) VALUES (
    'Laptop', 1200.00, 5, 1
);

SELECT * FROM ecommerce.categorias;

SELECT * FROM ecommerce.productos;