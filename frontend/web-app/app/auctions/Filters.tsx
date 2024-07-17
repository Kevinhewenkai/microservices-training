import { Button, ButtonGroup } from 'flowbite-react';
import React from 'react'
import { useParamsStore } from '../hooks/useParamsStore';
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill, BsStopwatchFill } from 'react-icons/bs';
import { GiFinishLine, GiFlame } from 'react-icons/gi';

const pageSizeButton = [4, 8, 12]

const orderButtons = [
    {
        label: 'Alphabetical',
        icon: AiOutlineSortAscending,
        value: 'make'
    },
    {
        label: 'End Date',
        icon: AiOutlineClockCircle,
        value: 'endingSoon'
    },
    {
        label: 'Recently added',
        icon: BsFillStopCircleFill,
        value: 'new'
    },
]

const filterButtons = [
    {
        label: 'Live Auctions',
        icon: GiFlame,
        value: 'live'
    },
    {
        label: 'Ending < 6 hours',
        icon: GiFinishLine,
        value: 'endingSoon'
    },
    {
        label: 'completed',
        icon: BsStopwatchFill,
        value: 'finished'
    },
]

export default function Filters() {
    const pageSize = useParamsStore(state => state.pageSize)
    const setParams = useParamsStore(state => state.setParams)
    const orderBy = useParamsStore(state => state.orderBy)
    const filterBy = useParamsStore(state => state.filterBy)
  return (
    <>
        <div className='flex justify-between items-center mb-4'>
            <div>
                <span className=' uppercase text-sm text-gray-500 mr-2'>Filter By</span>
                <ButtonGroup>
                    {filterButtons.map(({label, icon: Icon, value}) => (
                        <Button
                            key={value}
                            onClick={() => {setParams({ filterBy: value})}}
                            color={`${filterBy === value ? 'red' : 'gray'}`}
                        >
                            <Icon className='mr-3 h-5 w-5' />
                            {label}
                        </Button>
                    ))}
                </ButtonGroup>
            </div>

            <div>
                <span className=' uppercase text-sm text-gray-500 mr-2'>Order By</span>
                <ButtonGroup>
                    {orderButtons.map(({label, icon: Icon, value}) => (
                        <Button
                            key={value}
                            onClick={() => {setParams({ orderBy: value})}}
                            color={`${orderBy === value ? 'red' : 'gray'}`}
                        >
                            <Icon className='mr-3 h-5 w-5' />
                            {label}
                        </Button>
                    ))}
                </ButtonGroup>
            </div>

            <div>
                <span className=' uppercase text-sm text-gray-500 mr-2'>Page Size</span>
                <ButtonGroup>
                    {pageSizeButton.map((value, index: number) => (
                        <Button 
                            key={index} 
                            onClick={() => {setParams({pageSize: value})}}
                            color={`${pageSize === value ? 'red' : 'gray'}`}
                        >
                            {value}
                        </Button>
                    ))}
                </ButtonGroup>
            </div>
        </div>
    </>
  )
}
