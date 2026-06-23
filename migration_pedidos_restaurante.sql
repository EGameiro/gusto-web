-- Adiciona restaurante_id na tabela pedidos para suporte multi-tenant

ALTER TABLE pedidos ADD COLUMN IF NOT EXISTS restaurante_id INT NOT NULL DEFAULT 1;

ALTER TABLE pedidos ADD INDEX IF NOT EXISTS idx_pedidos_restaurante (restaurante_id);
