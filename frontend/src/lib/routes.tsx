import { Route, Routes } from "react-router-dom";
import App from '../App'
import Past from '../Past'
import Request from '../Request'
import ImagePage from '../image'


const AppRoutes: React.FC = () => (
    <Routes>
        <Route path='/' element={<App />} />
        < Route path='/past' element={<Past />} />
        < Route path='/requests/:id' element={<Request />} />
        < Route path='/requests/:id/image' element={<ImagePage />} />


    </Routes>
);

export default AppRoutes;