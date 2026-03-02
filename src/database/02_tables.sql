SET search_path TO ecommerce;

CREATE TABLE categorias (
    id_categoria SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255)
);

CREATE TABLE productos (
    id_producto SERIAL PRIMARY KEY,
    nombre VARCHAR(150) NOT NULL,
    descripcion TEXT,
    precio DECIMAL(10,2) NOT NULL,
    stock INT NOT NULL,
    estado BOOLEAN DEFAULT TRUE,
    imagen VARCHAR(255),
    id_categoria INT
);


CREATE TABLE usuarios (
    id_usuario SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    contrasena_hash VARCHAR(255) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    rol VARCHAR(50),
    estado BOOLEAN DEFAULT TRUE
);

CREATE TABLE cliente (
    id_cliente SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    chat_id VARCHAR(100)
);

CREATE TABLE pedido (
    id_pedido SERIAL PRIMARY KEY,
    fecha DATE DEFAULT CURRENT_DATE,
    total DECIMAL(10,2),
    estado VARCHAR(50),
    id_cliente INT,
    id_usuario INT
);

CREATE TABLE conversaciones (
    id_sesion SERIAL PRIMARY KEY,
    datos_temporales_json JSONB,
    id_cliente INT
);

CREATE TABLE pagos (
    id_pago SERIAL PRIMARY KEY,
    metodo VARCHAR(50),
    estado VARCHAR(50),
    fecha_pago DATE,
    id_pedido INT
);