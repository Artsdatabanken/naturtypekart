import PropTypes from 'prop-types'
import React from 'react'
import { Card, CardActions, CardMedia, CardTitle, CardText } from 'material-ui/Card'
import FlatButton from 'material-ui/FlatButton'
import { Table, TableBody, TableHeader, TableHeaderColumn, TableRow, TableRowColumn } from 'material-ui/Table'

const koder = {
  'KA-1': {
    kode: 'KA-a',
    tittel: 'Svært kalkfattig',
    ingress: 'ingress...',
    mertekst: 'masse tekst.',
    foto: 'https://www.ngu.no/sites/default/files/styles/profilbilde/public/NGUW10303.jpg?itok=XmM5cPuK'
  },
  'KA-2': {
    kode: 'KA-b',
    tittel: 'litt kalkfattig',
    ingress: 'ingress...',
    mertekst: 'masse tekst.',
    foto: 'http://www.ngu.no/sites/default/files/styles/fullstorrelse/public/NGUW10494.jpg?itok=4nqvIvWd'
  },
  'KA-3': {
    kode: 'KA-d',
    tittel: 'intermediær',
    ingress: 'ingress...',
    mertekst: 'masse tekst.',
    foto: 'http://www.ngu.no/sites/default/files/styles/fullstorrelse/public/NGUW10493.jpg?itok=5CkQBhD0'
  },
  'KA-4': {
    kode: 'KA-g',
    tittel: 'temmelig kalkrik',
    ingress: 'ingress...',
    mertekst: 'masse tekst.',
    foto: 'https://www.ngu.no/sites/default/files/styles/profilbilde/public/NGUW10357.jpg?itok=8Pjz_yas'
  },
  'KA-5': {
    kode: 'KA-h',
    tittel: 'svært kalkrik',
    ingress: 'ingress...',
    mertekst: 'masse tekst.',
    foto: 'https://www.ngu.no/sites/default/files/styles/profilbilde/public/NGUW10365.jpg?itok=7OB7Mllf'
  }
}

class Kalkkort extends React.Component {
  static propTypes = {
    kode: PropTypes.string.isRequired,
    properties: PropTypes.any.isRequired
  };
  static listProperties (props) {
    return Object
      .keys(props)
      .map(key => (
        <TableRow key={key}>
          <TableRowColumn>
            { key }
          </TableRowColumn>
          <TableRowColumn>
            <b>{ props[key] }</b>
          </TableRowColumn>
        </TableRow>
      ))
  }

  render () {
    console.log(this.props.kode)
    const data = koder[this.props.kode]
    if (!data) {
      return <span />
    }
    const kprops = this.props.properties
    const title = kprops.TEGNFORKLA.replace('�', 'ø')
//    const title = kprops.UNDERBER_1 ? kprops.HOVEDBER_1 + " med " + kprops.UNDERBER_1 : kprops.HOVEDBER_1
    const subtitle = 'Kalkinnhold: ' + data.tittel + ' (' + data.kode + ')'
    return (
      <Card>
        <CardMedia
          actAsExpander
          showExpandableButton overlay={
            <CardTitle title={title} subtitle={subtitle} />}
        >
          <img src={data.foto} width={340} alt='todo' />
        </CardMedia>
        <CardText expandable>
          { data.ingress }
        </CardText>
        <CardText expandable>
          <Table multiSelectable={false}>
            <TableHeader displaySelectAll={false} adjustForCheckbox={false} enableSelectAll={false}>
              <TableHeaderColumn>Prop</TableHeaderColumn>
              <TableHeaderColumn>Val</TableHeaderColumn>
            </TableHeader>
            <TableBody displayRowCheckbox={false} stripedRows>
              { Kalkkort.listProperties(this.props.properties) }
            </TableBody>
          </Table>
        </CardText>
        <CardActions expandable>
          <FlatButton label='Gjør noe' />
          <FlatButton label='Eller ikke' />
        </CardActions>
      </Card>
    )
  }
}

export default Kalkkort
