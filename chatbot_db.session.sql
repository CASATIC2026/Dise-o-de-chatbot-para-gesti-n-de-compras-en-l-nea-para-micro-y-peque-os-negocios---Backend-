INSERT INTO "Usuarios" (
    "Nombre",
    "Email",
    "ContrasenaHash",
    "Rol",
    "Estado",
    "Telefono"
  )
VALUES (
    'Rodrigo',
    'rodrigo@example.com',
    '$2a$12$ChxjiH5YF31.ceHAcfeY7.F.Bq739Fs6FMEsX9ErS/kgbYGaIb4G6',
    1,
    true,
    '72665119'
);


INSERT INTO "Pagos" (
    "PedidoId",
    "Monto",
    "MetodoPago",
    "Estado",
    "ReferenciaTransaccion",
    "FechaPago",
    "CreadoEn",
    "ActualizadoEn"
)
VALUES (
    3,                       -- PedidoId (Asegúrate que el pedido exista)
    450.00,                  -- Monto (Menor a 1000 para que Wompi no de error)
    'TARJETA_CREDITO',       -- MetodoPago
    0,                       -- Estado (0 suele ser 'Pendiente')
    'REF-WMP-001',           -- ReferenciaTransaccion
    CURRENT_TIMESTAMP,       -- FechaPago
    CURRENT_TIMESTAMP,       -- CreadoEn
    CURRENT_TIMESTAMP        -- ActualizadoEn
);


INSERT INTO "Clientes" ("Id", "Nombre", "Email", "Telefono", "CreadoEn")
VALUES (1, 'Rodrigo Benitez', 'rodrigo@example.com', '7777-7777', CURRENT_TIMESTAMP)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Pedidos" (
    "Id",
    "UsuarioId",
    "ClienteId",
    "Estado",
    "Total",
    "DireccionEntrega",
    "DetallesJson",
    "ReferenciaWompi",
    "CreadoEn",
    "ActualizadoEn"
)
VALUES (
    3,
    1,                               -- UsuarioId
    1,                               -- ClienteId
    0,                               -- Estado (Pendiente)
    450.00,                          -- Total (Menor a 1000)
    'San Salvador, El Salvador',     -- Direccion
    '{"items": [{"id": 1, "qty": 1}]}',-- Detalles
    'REF-WOMPI-TEST-003',            -- Referencia
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);

INSERT INTO "Pagos" (
    "PedidoId",
    "Monto",
    "MetodoPago",
    "Estado",
    "ReferenciaTransaccion",
    "FechaPago",
    "CreadoEn",
    "ActualizadoEn"
)
VALUES (
    3, 
    450.00, 
    'TARJETA_CREDITO', 
    0, 
    'TRANS-003', 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP
);