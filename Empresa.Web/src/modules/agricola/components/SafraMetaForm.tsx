import React, { useEffect } from 'react';
import { Modal, Form, Select, InputNumber, DatePicker, Input } from 'antd';
import type { MetaCompra, MetaCompraRequest, EmpresaMeta } from '@/modules/agricola/types';
import dayjs from 'dayjs';

interface SafraMetaFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (request: MetaCompraRequest) => Promise<void>;
  empresas: EmpresaMeta[];
  editingMeta?: MetaCompra | null;
}

export const SafraMetaForm: React.FC<SafraMetaFormProps> = ({
  open,
  onClose,
  onSubmit,
  empresas,
  editingMeta,
}) => {
  const [form] = Form.useForm();

  useEffect(() => {
    if (editingMeta) {
      form.setFieldsValue({
        cdEmpresa: editingMeta.cdEmpresa,
        cdLinha: editingMeta.cdLinha,
        safra: editingMeta.safra,
        dtInicial: dayjs(editingMeta.dtInicial),
        dtFinal: dayjs(editingMeta.dtFinal),
        metaQtde: editingMeta.metaQtde,
      });
    } else {
      form.resetFields();
    }
  }, [editingMeta, form, open]);

  const handleSubmit = async () => {
    const values = await form.validateFields();
    const request: MetaCompraRequest = {
      cdEmpresa: values.cdEmpresa,
      cdLinha: values.cdLinha,
      safra: values.safra,
      dtInicial: values.dtInicial?.toISOString(),
      dtFinal: values.dtFinal?.toISOString(),
      metaQtde: values.metaQtde,
    };
    await onSubmit(request);
    form.resetFields();
    onClose();
  };

  return (
    <Modal
      title={editingMeta ? 'Editar Meta' : 'Nova Safra/Meta'}
      open={open}
      onOk={handleSubmit}
      onCancel={() => {
        form.resetFields();
        onClose();
      }}
      destroyOnClose
    >
      <Form form={form} layout="vertical">
        <Form.Item
          name="cdEmpresa"
          label="Empresa"
          rules={[{ required: true, message: 'Informe a Empresa' }]}
        >
          <Select>
            {empresas.map((e) => (
              <Select.Option key={e.cdEmpresa} value={e.cdEmpresa}>
                {e.nome}
              </Select.Option>
            ))}
          </Select>
        </Form.Item>
        <Form.Item
          name="cdLinha"
          label="Linha"
          rules={[{ required: true, message: 'Informe uma Linha' }]}
        >
          <InputNumber style={{ width: '100%' }} min={1} />
        </Form.Item>
        <Form.Item
          name="safra"
          label="Safra"
          rules={[{ required: true, message: 'Informe uma Safra' }]}
        >
          <Input placeholder="Ex: 2026" />
        </Form.Item>
        <Form.Item
          name="dtInicial"
          label="Data Inicial"
          rules={[{ required: true, message: 'Informe a Data de Inicio da Safra' }]}
        >
          <DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item
          name="dtFinal"
          label="Data Final"
          rules={[
            { required: true, message: 'Informe a Data final da Safra' },
            ({ getFieldValue }) => ({
              validator(_rule: unknown, value: unknown) {
                const dtInicial = getFieldValue('dtInicial');
                const dateValue = value as dayjs.Dayjs | undefined;
                if (dateValue && dtInicial && dateValue.isBefore(dtInicial as dayjs.Dayjs, 'day')) {
                  return Promise.reject(new Error('A Data Final deve ser maior ou igual à Data Inicial.'));
                }
                return Promise.resolve();
              },
            }),
          ]}
        >
          <DatePicker format="DD/MM/YYYY" style={{ width: '100%' }} />
        </Form.Item>
        <Form.Item
          name="metaQtde"
          label="Meta (Qtde)"
          rules={[{ required: true, message: 'Informe a Meta a Ser Cadastrada' }]}
        >
          <InputNumber style={{ width: '100%' }} min={0} />
        </Form.Item>
      </Form>
    </Modal>
  );
};
