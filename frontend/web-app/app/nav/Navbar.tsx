import React from 'react'
import { FaCarSide } from "react-icons/fa";
import Search from './Search';
import Logo from './Logo';

export default function Navbar() {
  return (
    <header className='
      sticky top-0 z-50 flex justify-between bg-white 
      p-5 items-center text-gray-800 shadow-md
    '>
        <div>
          <Logo />
        </div>
        <Search />
        <div>Right</div>
    </header>
  )
}
