import { Route, Routes } from "react-router-dom";
import App from '../App'
import Past from '../Past'
import Request from '../Request'

const AppRoutes: React.FC = () => (
    <Routes>
        <Route path='/' element={<App />} />
        < Route path='/past' element={<Past />} />
        < Route path='/requests/:id' element={<Request />} />

    </Routes>
);

export default AppRoutes;