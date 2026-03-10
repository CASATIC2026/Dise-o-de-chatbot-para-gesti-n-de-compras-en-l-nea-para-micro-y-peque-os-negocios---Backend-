SET search_path TO ecommerce;

CREATE INDEX idx_producto_categoria ON productos(id_categoria);
CREATE INDEX idx_pedido_cliente ON pedido(id_cliente);
CREATE INDEX idx_conversacion_cliente ON conversaciones(id_cliente);INSERT INTO productos (
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
    id_producto:integer,
    'nombre:character varying',
    'descripcion:text',
    precio:numeric,
    stock:integer,
    estado:boolean,
    'imagen:character varying',
    id_categoria:integer
  );