import { CalculatorForm } from './components/CalculatorForm'
import './App.css'

function App() {
  return (
    <main className="app">
      <header className="app-header">
        <h1>Probability Calculator</h1>
        <p>
          Enter two probabilities between 0 and 1, choose an operation, and
          calculate the combined probability.
        </p>
      </header>
      <CalculatorForm />
    </main>
  )
}

export default App
