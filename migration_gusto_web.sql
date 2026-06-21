-- ============================================================
-- GUSTO Convênio Web — Migration inicial
-- Executar via phpMyAdmin no banco compartilhado do gusto-agent
-- Idempotente: usa IF NOT EXISTS / IF NOT EXISTS para colunas
-- ============================================================

-- ------------------------------------------------------------
-- 1. Colunas adicionais em empresas_convenio
-- ------------------------------------------------------------
ALTER TABLE empresas_convenio
    ADD COLUMN email          VARCHAR(150)  NULL,
    ADD COLUMN senha_hash     VARCHAR(256)  NULL,
    ADD COLUMN horario_corte  TIME          NOT NULL DEFAULT '10:00:00';

-- ------------------------------------------------------------
-- 2. Funcionários (por tenant)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS funcionarios (
    id          INT           NOT NULL AUTO_INCREMENT,
    empresa_id  INT           NOT NULL,
    nome        VARCHAR(200)  NOT NULL,
    ativo       TINYINT(1)    NOT NULL DEFAULT 1,
    criado_em   DATETIME      NOT NULL DEFAULT NOW(),

    PRIMARY KEY (id),
    INDEX idx_funcionarios_empresa (empresa_id),
    CONSTRAINT fk_funcionarios_empresa
        FOREIGN KEY (empresa_id) REFERENCES empresas_convenio(id)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ------------------------------------------------------------
-- 3. Cardápio web (sem tenant — compartilhado entre todos)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS cardapio_web (
    id          INT           NOT NULL AUTO_INCREMENT,
    dia_semana  TINYINT       NOT NULL COMMENT '0=Seg, 1=Ter, 2=Qua, 3=Qui, 4=Sex, 5=Sab',
    tipo        VARCHAR(20)   NOT NULL COMMENT 'prato | acompanhamento',
    nome        VARCHAR(200)  NOT NULL,
    ativo       TINYINT(1)    NOT NULL DEFAULT 1,
    ordem       INT           NOT NULL DEFAULT 0,

    PRIMARY KEY (id),
    INDEX idx_cardapio_dia_tipo (dia_semana, tipo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ------------------------------------------------------------
-- 4. Administradores do restaurante (fora do tenant)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS admin_users (
    id          INT           NOT NULL AUTO_INCREMENT,
    nome        VARCHAR(200)  NOT NULL,
    email       VARCHAR(150)  NOT NULL,
    senha_hash  VARCHAR(256)  NOT NULL,
    ativo       TINYINT(1)    NOT NULL DEFAULT 1,
    criado_em   DATETIME      NOT NULL DEFAULT NOW(),

    PRIMARY KEY (id),
    UNIQUE KEY uq_admin_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
