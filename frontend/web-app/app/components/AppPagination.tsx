'use client'

import { Pagination } from 'flowbite-react'

type Props = {
    currentPage: number,
    pageCount: number
    onPageChange: (page: number) => void
}

export default function AppPagination({currentPage, pageCount, onPageChange} : Props) {

  return (
    <Pagination 
      currentPage={currentPage} 
      onPageChange={onPageChange}
      totalPages={pageCount}
      layout='pagination'
      showIcons={true}
      className=' text-blue-500 mb-5'
    />
  )
}
