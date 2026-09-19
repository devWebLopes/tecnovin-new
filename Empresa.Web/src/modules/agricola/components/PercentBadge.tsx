import React from 'react';
import { Tag } from 'antd';
import { getSemaforoCor, formatPercent } from '../utils/comprasFrutasFormat';

interface PercentBadgeProps {
  percentual: number | null | undefined;
  showIcon?: boolean;
}

export const PercentBadge: React.FC<PercentBadgeProps> = ({ percentual, showIcon = true }) => {
  const semaforo = getSemaforoCor(percentual);
  const textoFormatado = formatPercent(percentual);

  return (
    <Tag
      style={{
        backgroundColor: semaforo.bg,
        color: semaforo.color,
        fontWeight: 'bold',
        fontSize: '13px',
        padding: '2px 8px',
        borderRadius: '4px',
        border: 'none',
        display: 'inline-flex',
        alignItems: 'center',
        gap: '4px',
        whiteSpace: 'nowrap',
      }}
    >
      <span>{textoFormatado}</span>
      {showIcon && semaforo.icon !== '—' && <span>{semaforo.icon}</span>}
    </Tag>
  );
};
