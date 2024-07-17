'use client'

import React from 'react'
import { FaCarSide } from 'react-icons/fa'
import { useParamsStore } from '../hooks/useParamsStore'

export default function Logo() {
    const reset = useParamsStore(s => s.reset)

  return (
    <div onClick={reset} className=' cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-400'>
        <FaCarSide size={34} />
        <div>Kemios Car Auction</div>
    </div>
  )
}
