import { Route, Routes } from "react-router-dom";
import App from '../App'
import Past from '../Past'

const AppRoutes: React.FC = () => (
    <Routes>
        <Route path='/' element={<App />} />
        < Route path='/past' element={<Past />} />
    </Routes>
);

export default AppRoutes;