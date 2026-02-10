import categories from '../data/categories';

const Home = () => {
  return (
    <div>
      <h1>Home Page</h1>
      <ul>
        {categories.map(cat => (
          <li key={cat.id}>{cat.name}</li>
        ))}
      </ul>
    </div>
  );
};

export default Home;