import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import MyTestComponent from './Components/MyTestComponent'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <MyTestComponent url={'https://catfact.ninja/facts?limit=15'}/>
    </>
  )
}

export default App
