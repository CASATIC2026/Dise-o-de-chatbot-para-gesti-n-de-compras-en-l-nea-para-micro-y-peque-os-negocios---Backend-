INSERT INTO usuarios (
    nombre, 
    email, 
    contrasena_hash, 
    rol, 
    estado
    ) 
VALUES (
    'Rodrigo', 
    'admin@example.com', 
    '$2a$12$O9Hp233xTnuHIdD5ODJbkOJYp3KoJxP0KDiAOX4azbZR11j5ey/bG', 
    'Admin', 
    true
);

INSERT usuarios INTO productos (
    id_producto,
    nombre,
    descripcion,
    precio,
    stock,
    estado,
    imagen,
    id_categoria
  )
VALUES (
    1,
    'Producto de Prueba',
    'descripcion:text',
    125.50,
    10,
    true,
    'imagen:character varying',
    id_categoria:integer
  );


  INSERT INTO productos (
      id_producto,
      nombre,
      descripcion,
      precio,
      stock,
      estado,
      imagen,
      id_categoria
    )
  VALUES (
      1,
      'Producto de Prueba',
      'descripcion:text',
      125.50,
      10,
      true,
      'imagen:character varying',
      1
    );

    SELECT * FROM usuarios  