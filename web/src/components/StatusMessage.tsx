import type { ReactNode } from 'react'

interface StatusMessageProps {
  testId: string
  tone?: 'info' | 'error'
  children: ReactNode
  action?: { label: string; onClick: () => void; testId: string }
}

export default function StatusMessage({ testId, tone = 'info', children, action }: StatusMessageProps) {
  return (
    <div className={`status status-${tone}`} data-testid={testId} role={tone === 'error' ? 'alert' : 'status'}>
      <p>{children}</p>
      {action && (
        <button type="button" className="button" onClick={action.onClick} data-testid={action.testId}>
          {action.label}
        </button>
      )}
    </div>
  )
}
