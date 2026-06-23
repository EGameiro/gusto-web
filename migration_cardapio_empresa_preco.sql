-- Migration: cardápio por empresa + campo preço
-- Executar via phpMyAdmin

ALTER TABLE cardapio_web
    ADD COLUMN IF NOT EXISTS empresa_id INT NULL DEFAULT NULL,
    ADD COLUMN IF NOT EXISTS preco DECIMAL(10,2) NULL DEFAULT NULL;

ALTER TABLE cardapio_web
    ADD INDEX IF NOT EXISTS idx_cardapio_empresa (empresa_id);
