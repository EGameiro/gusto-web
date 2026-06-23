-- Adiciona restaurante_id na tabela pedidos para suporte multi-tenant
CALL gusto_migrate();

DROP PROCEDURE IF EXISTS gusto_migrate;
CREATE PROCEDURE gusto_migrate()
BEGIN
    -- pedidos.restaurante_id
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pedidos' AND COLUMN_NAME = 'restaurante_id'
    ) THEN
        ALTER TABLE pedidos ADD COLUMN restaurante_id INT NOT NULL DEFAULT 1;
        ALTER TABLE pedidos ADD INDEX idx_pedidos_restaurante (restaurante_id);
    END IF;
END;

CALL gusto_migrate();
DROP PROCEDURE IF EXISTS gusto_migrate;
