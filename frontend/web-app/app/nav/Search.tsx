'use client'

import React, { useState } from 'react'
import { FaSearch } from 'react-icons/fa'
import { useParamsStore } from '../hooks/useParamsStore'
import { usePathname, useRouter } from 'next/navigation'

export default function Search() {
    const router = useRouter()
    const pathname = usePathname()
    const setParams = useParamsStore(e => e.setParams)
    const setSearchValue = useParamsStore(e => e.setSearchValue)
    const searchValue = useParamsStore(e => e.searchValue)

    const onChange = (event: any) => {
        setSearchValue(event.target.value)
    }

    const search = () => {
        if (pathname !== '/') router.push('/');
        setParams({searchTerm: searchValue})
    }

  return (
    <div className='flex w-[50%] items-center border-2 rounded-full py-2 shadow-sm'>
        <input
            type='text'
            placeholder='Search for cars by make, model or color'
            onChange={onChange}
            onKeyDown={(e: any) => {
                if (e.key === 'Enter') search()
            }}
            value={searchValue}
            className='
                input-custom
                text-sm
                text-gray-600
            '
        />
        <button onClick={search}>
            <FaSearch size={34} className='
                bg-red-400
                text-white
                p-2
                rounded-full
                cursor-pointer
                mx-2
            ' />
        </button>

    </div>
  )
}
