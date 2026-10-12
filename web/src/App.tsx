import { useEffect, useState } from 'react'

// TODO: move to env
const API = 'http://localhost:5080'

export default function App() {
  const [books, setBooks] = useState<any>([])
  const [q, setQ] = useState('')
  const [cat, setCat] = useState('')
  const [selected, setSelected] = useState<any>(null)
  const [cart, setCart] = useState<any>([])
  const [showAdd, setShowAdd] = useState(false)
  const [newBook, setNewBook] = useState<any>({})

  useEffect(() => {
    fetch(API + '/api/getBooks').then(r => r.json()).then(d => setBooks(d))
  }, [])

  function search() {
    if (q == '') {
      fetch(API + '/api/getBooks').then(r => r.json()).then(d => setBooks(d))
    } else {
      fetch(API + '/api/searchBooks?q=' + q).then(r => r.json()).then(d => setBooks(d))
    }
  }

  function filterCat(c) {
    setCat(c)
    if (c == '') { search(); return }
    fetch(API + '/api/books/category/' + c).then(r => r.json()).then(d => setBooks(d))
  }

  function addToCart(b) {
    // cart is only in memory for now
    setCart([...cart, b])
    alert('Added ' + b.title + ' to cart!')
  }

  function total() {
    let t = 0
    for (let i = 0; i < cart.length; i++) {
      t = t + cart[i].price
    }
    // 18% gst?
    return t * 1.18
  }

  function save() {
    fetch(API + '/api/addBook', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...newBook, price: parseFloat(newBook.price), stock: parseInt(newBook.stock) })
    }).then(() => { setShowAdd(false); search() })
  }

  return (
    <div style={{ fontFamily: 'Arial', padding: 20, background: '#fafafa' }}>
      <h1 style={{ color: '#333' }}>📚 My Book Store</h1>
      <div style={{ float: 'right', border: '1px solid #ccc', padding: 10 }}>
        Cart: {cart.length} items | Total: Rs.{total().toFixed(2)}
      </div>
      <div>
        <input value={q} onChange={e => setQ(e.target.value)} placeholder="search..." />
        <button onClick={search}>Go</button>
        <select value={cat} onChange={e => filterCat(e.target.value)} style={{ marginLeft: 10 }}>
          <option value="">All</option>
          <option>Fiction</option>
          <option>Science Fiction</option>
          <option>Fantasy</option>
          <option>Mystery</option>
          <option>Technology</option>
          <option>Business</option>
          <option>History</option>
        </select>
        <button onClick={() => setShowAdd(!showAdd)} style={{ marginLeft: 10 }}>+ Add book</button>
      </div>

      {showAdd && (
        <div style={{ background: '#fff', padding: 10, margin: 10, border: '1px dashed #999' }}>
          <input placeholder="title" onChange={e => setNewBook({ ...newBook, title: e.target.value })} />
          <input placeholder="author" onChange={e => setNewBook({ ...newBook, author: e.target.value })} />
          <input placeholder="price" onChange={e => setNewBook({ ...newBook, price: e.target.value })} />
          <input placeholder="category" onChange={e => setNewBook({ ...newBook, category: e.target.value })} />
          <input placeholder="stock" onChange={e => setNewBook({ ...newBook, stock: e.target.value })} />
          <button onClick={save}>Save</button>
        </div>
      )}

      <div style={{ display: 'flex', flexWrap: 'wrap', marginTop: 20 }}>
        {books.map(b => (
          <div key={Math.random()} onClick={() => setSelected(b)}
            style={{ width: 200, margin: 8, padding: 10, background: 'white', boxShadow: '0 1px 3px #aaa', cursor: 'pointer' }}>
            <b>{b.title}</b>
            <div style={{ fontSize: 12, color: 'gray' }}>{b.author}</div>
            <div style={{ color: 'green' }}>Rs.{b.price.toFixed(2)}</div>
            {b.price > 500 && <span style={{ color: 'red', fontSize: 10 }}>5% OFF!!</span>}
            <button onClick={(e) => { e.stopPropagation(); addToCart(b) }}>Add to cart</button>
          </div>
        ))}
      </div>

      {selected && (
        <div style={{ position: 'fixed', top: 100, left: 300, background: 'white', padding: 20, border: '2px solid black' }}>
          <h2>{selected.title}</h2>
          <p>{selected.desc}</p>
          <p>By {selected.author} ({selected.year})</p>
          <p>ISBN: {selected.isbn}</p>
          <p>In stock: {selected.stock}</p>
          <button onClick={() => setSelected(null)}>close</button>
        </div>
      )}
    </div>
  )
}
