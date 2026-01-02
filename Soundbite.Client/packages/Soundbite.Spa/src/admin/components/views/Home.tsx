import React from "react";
import { observer } from "mobx-react-lite";
import { Link } from "react-router-dom";
import { Card, CardHeader, Row, Col } from "reactstrap";

import Layout from "../../components/Layout";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////
interface IProps {
  children?: React.ReactNode;
}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

const Component: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////
  return (
    <Layout>
      <Card>
        <CardHeader className="bg-transparent">
          <Row className="align-items-center">
            <Col>
              <h1 className="mb-0">Tenant Administration</h1>
            </Col>
          </Row>
        </CardHeader>
        <div className="table-responsive">
          <ul>
            <li>
              <Link to="/admin/orgs/">Organizations</Link>
            </li>
          </ul>
        </div>
      </Card>
    </Layout>
  );
});

export default Component;
