import React, { useEffect, useMemo, useState } from 'react';
import { Table, Button, Space, Tag, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { useSafraMeta } from '@/modules/agricola/hooks/useSafraMeta';
import { SafraMetaForm } from '@/modules/agricola/components/SafraMetaForm';
import type { MetaCompra, MetaCompraRequest } from '@/modules/agricola/types';

export const SafraMetaPage: React.FC = () => {
  const { metas, empresas, loading, loadMetas, loadEmpresas, createMeta, updateMeta, deleteMeta } = useSafraMeta();
  const [formOpen, setFormOpen] = useState(false);
  const [editingMeta, setEditingMeta] = useState<MetaCompra | null>(null);

  useEffect(() => {
    loadMetas();
    loadEmpresas();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreate = async (request: MetaCompraRequest) => {
    try {
      await createMeta(request);
      message.success('Meta cadastrada com sucesso!');
    } catch {
      message.error('Erro ao criar meta');
    }
  };

  const handleUpdate = async (request: MetaCompraRequest) => {
    if (!editingMeta) return;
    try {
      await updateMeta(editingMeta.idMetaCompra, request);
      message.success('Meta atualizada com sucesso!');
      setEditingMeta(null);
    } catch {
      message.error('Erro ao atualizar meta');
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteMeta(id);
      message.success('Meta excluída com sucesso!');
    } catch {
      message.error('Erro ao excluir meta');
    }
  };

  const sortedMetas = useMemo(() => {
    return [...metas].sort((a, b) => {
      const safraA = parseInt(a.safra, 10) || 0;
      const safraB = parseInt(b.safra, 10) || 0;
      if (safraB !== safraA) return safraB - safraA;
      if (a.cdEmpresa !== b.cdEmpresa) return a.cdEmpresa - b.cdEmpresa;
      return a.cdLinha - b.cdLinha;
    });
  }, [metas]);

  const columns: ColumnsType<MetaCompra> = [
    {
      title: 'EMPRESA',
      dataIndex: 'cdEmpresa',
      key: 'empresa',
      render: (cd: number) => empresas.find((e: { cdEmpresa: number; nome: string }) => e.cdEmpresa === cd)?.nome ?? cd,
    },
    { title: 'LINHA', dataIndex: 'cdLinha', key: 'cdLinha', width: 80 },
    {
      title: 'SAFRA',
      dataIndex: 'safra',
      key: 'safra',
      width: 130,
      sorter: (a, b) => (parseInt(a.safra, 10) || 0) - (parseInt(b.safra, 10) || 0),
      defaultSortOrder: 'descend',
      render: (safra: string) => {
        const isCurrent = safra === dayjs().year().toString();
        return (
          <Space size={6}>
            <span style={{ fontWeight: isCurrent ? 700 : 'normal' }}>{safra}</span>
            {isCurrent && (
              <Tag color="blue" style={{ margin: 0, fontWeight: 'normal' }}>
                Atual
              </Tag>
            )}
          </Space>
        );
      },
    },
    {
      title: 'DATA INICIAL',
      dataIndex: 'dtInicial',
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: 'DATA FINAL',
      dataIndex: 'dtFinal',
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
    {
      title: 'META',
      dataIndex: 'metaQtde',
      render: (v: number) => v?.toLocaleString('pt-BR'),
      width: 120,
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 120,
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={() => {
              setEditingMeta(record);
              setFormOpen(true);
            }}
          />
          <Popconfirm
            title="Deseja Excluir a Meta?"
            onConfirm={() => handleDelete(record.idMetaCompra)}
          >
            <Button type="link" danger size="small" icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const rowClassName = (record: MetaCompra) => {
    const currentYear = dayjs().year().toString();
    return record.safra === currentYear ? 'row-safra-atual' : '';
  };

  return (
    <div>
      <h2>Cadastro de Safra / Meta</h2>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={() => {
          setEditingMeta(null);
          setFormOpen(true);
        }}
        style={{ marginBottom: 16 }}
      >
        Nova Safra/Meta
      </Button>
      <Table
        columns={columns}
        dataSource={sortedMetas}
        rowKey="idMetaCompra"
        loading={loading}
        pagination={{ pageSize: 200 }}
        rowClassName={rowClassName}
        locale={{ emptyText: 'Nenhuma meta cadastrada.' }}
      />
      <SafraMetaForm
        open={formOpen}
        onClose={() => {
          setFormOpen(false);
          setEditingMeta(null);
        }}
        onSubmit={editingMeta ? handleUpdate : handleCreate}
        empresas={empresas}
        editingMeta={editingMeta}
      />
    </div>
  );
};

export default SafraMetaPage;
