-- ============================================================
-- MIGRAÇÃO: Multi-Restaurante
-- ============================================================

-- 1. Cria tabela restaurantes
-- ============================================================
CREATE TABLE IF NOT EXISTS restaurantes (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    nome        VARCHAR(200) NOT NULL,
    slug        VARCHAR(100) NOT NULL UNIQUE,
    telefone    VARCHAR(20)  NULL,
    email       VARCHAR(150) NULL,
    endereco    VARCHAR(300) NULL,
    ativo       TINYINT(1)   NOT NULL DEFAULT 1,
    criado_em   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Insere o GUSTO como primeiro restaurante
-- ============================================================
INSERT IGNORE INTO restaurantes (id, nome, slug, ativo)
VALUES (1, 'GUSTO', 'gusto', 1);

-- 3. Adiciona colunas via procedure (idempotente)
-- ============================================================
DROP PROCEDURE IF EXISTS gusto_migrate;

DELIMITER $$
CREATE PROCEDURE gusto_migrate()
BEGIN
    -- admin_users.restaurante_id
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'admin_users' AND COLUMN_NAME = 'restaurante_id'
    ) THEN
        ALTER TABLE admin_users ADD COLUMN restaurante_id INT NOT NULL DEFAULT 1;
    END IF;

    -- admin_users.is_super_admin
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'admin_users' AND COLUMN_NAME = 'is_super_admin'
    ) THEN
        ALTER TABLE admin_users ADD COLUMN is_super_admin TINYINT(1) NOT NULL DEFAULT 0;
    END IF;

    -- empresas_convenio.restaurante_id
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'empresas_convenio' AND COLUMN_NAME = 'restaurante_id'
    ) THEN
        ALTER TABLE empresas_convenio ADD COLUMN restaurante_id INT NOT NULL DEFAULT 1;
    END IF;

    -- cardapio_web.restaurante_id
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'cardapio_web' AND COLUMN_NAME = 'restaurante_id'
    ) THEN
        ALTER TABLE cardapio_web ADD COLUMN restaurante_id INT NOT NULL DEFAULT 1;
    END IF;

    -- funcionarios.restaurante_id
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'funcionarios' AND COLUMN_NAME = 'restaurante_id'
    ) THEN
        ALTER TABLE funcionarios ADD COLUMN restaurante_id INT NOT NULL DEFAULT 1;
    END IF;

    -- Índices
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'admin_users' AND INDEX_NAME = 'idx_admin_restaurante'
    ) THEN
        ALTER TABLE admin_users ADD INDEX idx_admin_restaurante (restaurante_id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'empresas_convenio' AND INDEX_NAME = 'idx_empresa_restaurante'
    ) THEN
        ALTER TABLE empresas_convenio ADD INDEX idx_empresa_restaurante (restaurante_id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'cardapio_web' AND INDEX_NAME = 'idx_cardapio_restaurante'
    ) THEN
        ALTER TABLE cardapio_web ADD INDEX idx_cardapio_restaurante (restaurante_id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'funcionarios' AND INDEX_NAME = 'idx_func_restaurante'
    ) THEN
        ALTER TABLE funcionarios ADD INDEX idx_func_restaurante (restaurante_id);
    END IF;

END$$
DELIMITER ;

CALL gusto_migrate();
DROP PROCEDURE IF EXISTS gusto_migrate;

-- 4. Marca o admin atual como SuperAdmin
-- ============================================================
UPDATE admin_users SET is_super_admin = 1 WHERE email = 'eduardo@digiplay.net.br';
