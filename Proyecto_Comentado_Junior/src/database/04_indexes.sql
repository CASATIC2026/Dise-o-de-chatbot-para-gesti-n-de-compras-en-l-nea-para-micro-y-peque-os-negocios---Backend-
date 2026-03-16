SET search_path TO ecommerce;

CREATE INDEX idx_producto_categoria ON productos(id_categoria);
CREATE INDEX idx_pedido_cliente ON pedido(id_cliente);
CREATE INDEX idx_conversacion_cliente ON conversaciones(id_cliente);