import { AreaChart, Area, YAxis, ResponsiveContainer } from 'recharts'

interface Props {
  data: number[]
  color?: string
  height?: number
}

export default function Sparkline({ data, color = '#6366f1', height = 40 }: Props) {
  if (data.length < 2) return <div style={{ height, width: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--text-muted)', fontSize: '0.7rem' }}>Waiting...</div>
  const points = data.map(v => ({ v }))
  const min = Math.min(...data)
  const max = Math.max(...data)
  const pad = (max - min) * 0.1 || 1
  return (
    <ResponsiveContainer width="100%" height={height}>
      <AreaChart data={points} margin={{ top: 2, right: 0, bottom: 2, left: 0 }}>
        <defs>
          <linearGradient id={`g-${color.replace('#', '')}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={color} stopOpacity={0.3} />
            <stop offset="100%" stopColor={color} stopOpacity={0} />
          </linearGradient>
        </defs>
        <YAxis domain={[min - pad, max + pad]} hide />
        <Area type="monotone" dataKey="v" stroke={color} strokeWidth={1.5} fill={`url(#g-${color.replace('#', '')})`} dot={false} isAnimationActive={false} />
      </AreaChart>
    </ResponsiveContainer>
  )
}
