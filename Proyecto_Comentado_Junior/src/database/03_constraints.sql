SET search_path TO ecommerce;

ALTER TABLE productos
ADD CONSTRAINT fk_producto_categoria
FOREIGN KEY (id_categoria)
REFERENCES categorias(id_categoria);

ALTER TABLE pedido
ADD CONSTRAINT fk_pedido_cliente
FOREIGN KEY (id_cliente)
REFERENCES cliente(id_cliente);

ALTER TABLE pedido
ADD CONSTRAINT fk_pedido_usuario
FOREIGN KEY (id_usuario)
REFERENCES usuarios(id_usuario);

ALTER TABLE conversaciones
ADD CONSTRAINT fk_conversacion_cliente
FOREIGN KEY (id_cliente)
REFERENCES cliente(id_cliente);

ALTER TABLE pagos
ADD CONSTRAINT fk_pago_pedido
FOREIGN KEY (id_pedido)
REFERENCES pedido(id_pedido);